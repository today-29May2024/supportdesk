using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupportDesk.Core.Dtos;
using SupportDesk.Infrastructure.Data;

namespace SupportDesk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public AgentsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AgentDto>>> GetAgents([FromQuery] string? search)
    {
        var query = _context.Agents.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(a => a.FullName.ToLower().Contains(term) || a.Email.ToLower().Contains(term));
        }

        var agents = await query
            .Select(a => new AgentDto(a.Id, a.FullName, a.Email, a.Department, a.Active))
            .ToListAsync();

        return Ok(agents);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AgentDto>> GetAgentById(int id)
    {
        var agent = await _context.Agents.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
        if (agent == null) return NotFound(new { error = "NOT_FOUND", message = $"Agent with ID '{id}' was not found." });

        return Ok(new AgentDto(agent.Id, agent.FullName, agent.Email, agent.Department, agent.Active));
    }
}