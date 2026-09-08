using Microsoft.EntityFrameworkCore;
using SupportDesk.Core.Dtos;
using SupportDesk.Core.Entities;
using SupportDesk.Core.Enums;
using SupportDesk.Core.Exceptions;
using SupportDesk.Core.Services;
using SupportDesk.Infrastructure.Data;

namespace SupportDesk.Api.Services;

public class TicketService : ITicketService
{
    private readonly AppDbContext _context;

    public TicketService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResultDto<TicketListDto>> GetTicketsAsync(TicketFilterParams filter)
    {
        var query = _context.Tickets
            .Include(t => t.AssignedAgent)
            .AsNoTracking()
            .AsQueryable();

        // 1. Search Filter (Reference, Title, Customer)
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim().ToLower();
            query = query.Where(t => 
                t.Reference.ToLower().Contains(search) ||
                t.Title.ToLower().Contains(search) ||
                t.CustomerName.ToLower().Contains(search));
        }

        // 2. Status & Priority Filters
        if (filter.Status.HasValue)
        {
            query = query.Where(t => t.Status == filter.Status.Value);
        }

        if (filter.Priority.HasValue)
        {
            query = query.Where(t => t.Priority == filter.Priority.Value);
        }

        // 3. Agent Filter
        if (filter.AssignedAgentId.HasValue)
        {
            query = query.Where(t => t.AssignedAgentId == filter.AssignedAgentId.Value);
        }

        // 4. Overdue Only Filter (DueDate < UtcNow and status not Resolved/Closed)
        if (filter.OverdueOnly)
        {
            var now = DateTime.UtcNow;
            query = query.Where(t => t.DueDate < now && t.Status != TicketStatus.Resolved && t.Status != TicketStatus.Closed);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(t => t.CreatedDate)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(t => new TicketListDto(
                t.Id,
                t.Reference,
                t.Title,
                t.CustomerName,
                t.Priority,
                t.Status,
                t.AssignedAgent != null ? t.AssignedAgent.FullName : null,
                t.AssignedAgentId,
                t.DueDate,
                t.IsOverdue
            ))
            .ToListAsync();

        return new PagedResultDto<TicketListDto>(items, totalCount, filter.Page, filter.PageSize);
    }

    public async Task<TicketDetailDto> GetTicketByIdAsync(int id)
    {
        var ticket = await _context.Tickets
            .Include(t => t.AssignedAgent)
            .Include(t => t.Comments)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new KeyNotFoundException($"Ticket with ID '{id}' was not found.");

        return MapToDetailDto(ticket);
    }

    public async Task<TicketDetailDto> CreateTicketAsync(CreateTicketDto dto)
    {
        var ticketCountThisYear = await _context.Tickets
            .Where(t => t.CreatedDate.Year == DateTime.UtcNow.Year)
            .CountAsync();

        var reference = $"TCK-{DateTime.UtcNow.Year}-{(ticketCountThisYear + 1):D4}";

        var ticket = new Ticket
        {
            Reference = reference,
            Title = dto.Title,
            Description = dto.Description,
            CustomerName = dto.CustomerName,
            CustomerEmail = dto.CustomerEmail,
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow
        };

        // Workflow SLA due date calculation via domain initialization
        ticket.Initialize(dto.Priority);

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();

        return MapToDetailDto(ticket);
    }

    public async Task<TicketDetailDto> UpdateTicketAsync(int id, UpdateTicketDto dto)
    {
        var ticket = await _context.Tickets
            .Include(t => t.AssignedAgent)
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new KeyNotFoundException($"Ticket with ID '{id}' was not found.");

        ticket.EnsureNotClosed();

        ticket.Title = dto.Title;
        ticket.Description = dto.Description;
        ticket.CustomerName = dto.CustomerName;
        ticket.CustomerEmail = dto.CustomerEmail;

        if (ticket.Priority != dto.Priority)
        {
            ticket.UpdatePriority(dto.Priority);
        }

        await _context.SaveChangesAsync();
        return MapToDetailDto(ticket);
    }

    public async Task DeleteTicketAsync(int id)
    {
        var ticket = await _context.Tickets.FindAsync(id)
            ?? throw new KeyNotFoundException($"Ticket with ID '{id}' was not found.");

        _context.Tickets.Remove(ticket);
        await _context.SaveChangesAsync();
    }

    public async Task<TicketDetailDto> AssignAgentAsync(int id, AssignAgentDto dto)
    {
        var ticket = await _context.Tickets
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new KeyNotFoundException($"Ticket with ID '{id}' was not found.");

        var agent = await _context.Agents.FindAsync(dto.AgentId)
            ?? throw new KeyNotFoundException($"Agent with ID '{dto.AgentId}' was not found.");

        ticket.AssignAgent(agent);
        await _context.SaveChangesAsync();

        // Reload agent navigation for DTO
        await _context.Entry(ticket).Reference(t => t.AssignedAgent).LoadAsync();
        return MapToDetailDto(ticket);
    }

    public async Task<TicketDetailDto> UnassignAgentAsync(int id)
    {
        var ticket = await _context.Tickets
            .Include(t => t.AssignedAgent)
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new KeyNotFoundException($"Ticket with ID '{id}' was not found.");

        ticket.UnassignAgent();
        await _context.SaveChangesAsync();

        return MapToDetailDto(ticket);
    }

    public async Task<TicketDetailDto> ChangeStatusAsync(int id, ChangeStatusDto dto)
    {
        var ticket = await _context.Tickets
            .Include(t => t.AssignedAgent)
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new KeyNotFoundException($"Ticket with ID '{id}' was not found.");

        ticket.TransitionStatus(dto.NewStatus);
        await _context.SaveChangesAsync();

        return MapToDetailDto(ticket);
    }

    public async Task<CommentDto> AddCommentAsync(int ticketId, CreateCommentDto dto)
    {
        var ticket = await _context.Tickets.FindAsync(ticketId)
            ?? throw new KeyNotFoundException($"Ticket with ID '{ticketId}' was not found.");

        ticket.EnsureNotClosed();

        var comment = new Comment
        {
            TicketId = ticketId,
            AuthorName = dto.AuthorName,
            Body = dto.Body,
            CreatedDate = DateTime.UtcNow
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        return new CommentDto(comment.Id, comment.TicketId, comment.AuthorName, comment.Body, comment.CreatedDate);
    }

    private static TicketDetailDto MapToDetailDto(Ticket ticket) => new(
        ticket.Id,
        ticket.Reference,
        ticket.Title,
        ticket.Description,
        ticket.CustomerName,
        ticket.CustomerEmail,
        ticket.Priority,
        ticket.Status,
        ticket.AssignedAgentId,
        ticket.AssignedAgent != null ? new AgentDto(ticket.AssignedAgent.Id, ticket.AssignedAgent.FullName, ticket.AssignedAgent.Email, ticket.AssignedAgent.Department, ticket.AssignedAgent.Active) : null,
        ticket.CreatedDate,
        ticket.LastModifiedDate,
        ticket.ResolvedDate,
        ticket.ClosedDate,
        ticket.DueDate,
        ticket.IsOverdue,
        ticket.Comments.Select(c => new CommentDto(c.Id, c.TicketId, c.AuthorName, c.Body, c.CreatedDate)).ToList()
    );
}