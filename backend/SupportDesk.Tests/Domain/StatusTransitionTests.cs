using SupportDesk.Core.Entities;
using SupportDesk.Core.Enums;
using SupportDesk.Core.Exceptions;
using Xunit;

namespace SupportDesk.Tests.Domain;

public class StatusTransitionTests
{
    private readonly Agent _activeAgent = new()
    {
        Id = 1,
        FullName = "Sarah Connor",
        Email = "sarah@support.com",
        Department = Department.Technical,
        Active = true
    };

    [Fact]
    public void TransitionStatus_NewToInProgress_FailsIfNoAgentAssigned()
    {
        // Arrange
        var ticket = new Ticket { Reference = "TCK-2026-0001", Title = "Unassigned Ticket" };

        // Act & Assert (Rule #3: Agent required for In Progress)
        var exception = Assert.Throws<BusinessRuleException>(() => ticket.TransitionStatus(TicketStatus.InProgress));
        Assert.Equal("AGENT_ASSIGNMENT_REQUIRED", exception.Code);
    }

    [Fact]
    public void TransitionStatus_NewToInProgress_SucceedsWhenAgentAssigned()
    {
        // Arrange
        var ticket = new Ticket { Reference = "TCK-2026-0001", Title = "Assigned Ticket" };
        ticket.AssignAgent(_activeAgent);

        // Act
        ticket.TransitionStatus(TicketStatus.InProgress);

        // Assert
        Assert.Equal(TicketStatus.InProgress, ticket.Status);
    }

    [Fact]
    public void TransitionStatus_InvalidTransition_NewToResolved_ThrowsException()
    {
        // Arrange
        var ticket = new Ticket { Reference = "TCK-2026-0001", Title = "Skip Status Ticket" };
        ticket.AssignAgent(_activeAgent);

        // Act & Assert (Rule #2: Skipping statuses directly from New -> Resolved is forbidden)
        var exception = Assert.Throws<BusinessRuleException>(() => ticket.TransitionStatus(TicketStatus.Resolved));
        Assert.Equal("INVALID_STATUS_TRANSITION", exception.Code);
    }

    [Fact]
    public void TransitionStatus_ClosedTicket_CannotBeReopened()
    {
        // Arrange
        var ticket = new Ticket { Reference = "TCK-2026-0001", Title = "Closed Ticket" };
        ticket.AssignAgent(_activeAgent);
        ticket.TransitionStatus(TicketStatus.InProgress);
        ticket.TransitionStatus(TicketStatus.Resolved);
        ticket.TransitionStatus(TicketStatus.Closed);

        // Act & Assert (Rule #2 & #5: Closed ticket cannot transition or reopen)
        var exception = Assert.Throws<BusinessRuleException>(() => ticket.TransitionStatus(TicketStatus.InProgress));
        Assert.Equal("TICKET_READ_ONLY", exception.Code);
    }
}