using SupportDesk.Core.Dtos;

namespace SupportDesk.Core.Services;

public interface ITicketService
{
    Task<PagedResultDto<TicketListDto>> GetTicketsAsync(TicketFilterParams filter);
    Task<TicketDetailDto> GetTicketByIdAsync(int id);
    Task<TicketDetailDto> CreateTicketAsync(CreateTicketDto dto);
    Task<TicketDetailDto> UpdateTicketAsync(int id, UpdateTicketDto dto);
    Task DeleteTicketAsync(int id);
    Task<TicketDetailDto> AssignAgentAsync(int id, AssignAgentDto dto);
    Task<TicketDetailDto> UnassignAgentAsync(int id);
    Task<TicketDetailDto> ChangeStatusAsync(int id, ChangeStatusDto dto);
    Task<CommentDto> AddCommentAsync(int ticketId, CreateCommentDto dto);
}