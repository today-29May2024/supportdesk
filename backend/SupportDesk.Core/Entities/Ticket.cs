using SupportDesk.Core.Enums;
using SupportDesk.Core.Exceptions;

namespace SupportDesk.Core.Entities;

public class Ticket
{
    public int Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;

    public TicketPriority Priority { get; private set; }
    public TicketStatus Status { get; private set; } = TicketStatus.New;

    public int? AssignedAgentId { get; private set; }
    public Agent? AssignedAgent { get; private set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedDate { get; private set; }
    public DateTime? ClosedDate { get; private set; }
    public DateTime DueDate { get; private set; }

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public bool IsOverdue => Status != TicketStatus.Resolved 
                          && Status != TicketStatus.Closed 
                          && DateTime.UtcNow > DueDate;

    public static DateTime CalculateDueDate(TicketPriority priority, DateTime fromDate) => priority switch
    {
        TicketPriority.Critical => fromDate.AddHours(4),
        TicketPriority.High => fromDate.AddDays(1),
        TicketPriority.Normal => fromDate.AddDays(3),
        TicketPriority.Low => fromDate.AddDays(7),
        _ => throw new ArgumentOutOfRangeException(nameof(priority))
    };

    public void Initialize(TicketPriority priority)
    {
        Priority = priority;
        DueDate = CalculateDueDate(priority, CreatedDate);
    }

    public void UpdatePriority(TicketPriority newPriority)
    {
        EnsureNotClosed();
        Priority = newPriority;
        DueDate = CalculateDueDate(newPriority, CreatedDate);
        LastModifiedDate = DateTime.UtcNow;
    }

    public void AssignAgent(Agent agent)
    {
        EnsureNotClosed();
        if (!agent.Active)
        {
            throw new BusinessRuleException("INACTIVE_AGENT", "Cannot assign an inactive agent to a ticket.");
        }

        AssignedAgentId = agent.Id;
        AssignedAgent = agent;
        LastModifiedDate = DateTime.UtcNow;
    }

    public void UnassignAgent()
    {
        EnsureNotClosed();
        if (Status == TicketStatus.InProgress)
        {
            throw new BusinessRuleException("AGENT_REQUIRED", "Cannot unassign an agent while ticket is In Progress.");
        }

        AssignedAgentId = null;
        AssignedAgent = null;
        LastModifiedDate = DateTime.UtcNow;
    }

    public void TransitionStatus(TicketStatus newStatus)
    {
        EnsureNotClosed();

        if (Status == newStatus) return;

        bool isValid = (Status, newStatus) switch
        {
            (TicketStatus.New, TicketStatus.InProgress) => true,
            (TicketStatus.InProgress, TicketStatus.Resolved) => true,
            (TicketStatus.Resolved, TicketStatus.Closed) => true,
            (TicketStatus.Resolved, TicketStatus.InProgress) => true,
            _ => false
        };

        if (!isValid)
        {
            throw new BusinessRuleException("INVALID_STATUS_TRANSITION", 
                $"Invalid status transition from '{Status}' to '{newStatus}'.");
        }

        if (newStatus == TicketStatus.InProgress && AssignedAgentId == null)
        {
            throw new BusinessRuleException("AGENT_ASSIGNMENT_REQUIRED", 
                "A ticket cannot move to 'In Progress' without an assigned active agent.");
        }

        Status = newStatus;
        LastModifiedDate = DateTime.UtcNow;

        if (newStatus == TicketStatus.Resolved)
        {
            ResolvedDate = DateTime.UtcNow;
        }
        else if (newStatus == TicketStatus.Closed)
        {
            ClosedDate = DateTime.UtcNow;
        }
    }

    public void EnsureNotClosed()
    {
        if (Status == TicketStatus.Closed)
        {
            throw new BusinessRuleException("TICKET_READ_ONLY", "Closed tickets are read-only and cannot be modified.");
        }
    }
}