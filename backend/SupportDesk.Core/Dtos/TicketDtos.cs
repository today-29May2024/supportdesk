using SupportDesk.Core.Enums;

namespace SupportDesk.Core.Dtos;

public record TicketListDto(
    int Id,
    string Reference,
    string Title,
    string CustomerName,
    TicketPriority Priority,
    TicketStatus Status,
    string? AssignedAgentName,
    int? AssignedAgentId,
    DateTime DueDate,
    bool IsOverdue
);

public record TicketDetailDto(
    int Id,
    string Reference,
    string Title,
    string Description,
    string CustomerName,
    string CustomerEmail,
    TicketPriority Priority,
    TicketStatus Status,
    int? AssignedAgentId,
    AgentDto? AssignedAgent,
    DateTime CreatedDate,
    DateTime LastModifiedDate,
    DateTime? ResolvedDate,
    DateTime? ClosedDate,
    DateTime DueDate,
    bool IsOverdue,
    IReadOnlyList<CommentDto> Comments
);

public record CreateTicketDto(
    string Title,
    string Description,
    string CustomerName,
    string CustomerEmail,
    TicketPriority Priority
);

public record UpdateTicketDto(
    string Title,
    string Description,
    string CustomerName,
    string CustomerEmail,
    TicketPriority Priority
);

public record ChangeStatusDto(
    TicketStatus NewStatus
);

public record AssignAgentDto(
    int AgentId
);

public record TicketFilterParams(
    string? Search = null,
    TicketStatus? Status = null,
    TicketPriority? Priority = null,
    int? AssignedAgentId = null,
    bool OverdueOnly = false,
    int Page = 1,
    int PageSize = 10
);