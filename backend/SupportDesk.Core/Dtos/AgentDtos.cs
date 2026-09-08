using SupportDesk.Core.Enums;

namespace SupportDesk.Core.Dtos;

public record AgentDto(
    int Id,
    string FullName,
    string Email,
    Department Department,
    bool Active
);