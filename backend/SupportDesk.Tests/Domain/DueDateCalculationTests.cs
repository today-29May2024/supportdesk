using SupportDesk.Core.Entities;
using SupportDesk.Core.Enums;
using Xunit;

namespace SupportDesk.Tests.Domain;

public class DueDateCalculationTests
{
    [Theory]
    [InlineData(TicketPriority.Critical, 4)]    // 4 hours
    [InlineData(TicketPriority.High, 24)]      // 1 day = 24 hours
    [InlineData(TicketPriority.Normal, 72)]    // 3 days = 72 hours
    [InlineData(TicketPriority.Low, 168)]      // 7 days = 168 hours
    public void CalculateDueDate_CorrectlyCalculatesFromCreationDate(TicketPriority priority, int expectedHours)
    {
        // Arrange
        var createdDate = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc);

        // Act
        var dueDate = Ticket.CalculateDueDate(priority, createdDate);

        // Assert
        var expectedDueDate = createdDate.AddHours(expectedHours);
        Assert.Equal(expectedDueDate, dueDate);
    }

    [Fact]
    public void UpdatePriority_RecalculatesDueDateFromOriginalCreationDate()
    {
        // Arrange
        var createdDate = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc);
        var ticket = new Ticket
        {
            Reference = "TCK-2026-0001",
            Title = "Test Ticket",
            CreatedDate = createdDate
        };
        ticket.Initialize(TicketPriority.Low); // Due date = created + 7 days

        // Act - Change priority from Low to Critical
        ticket.UpdatePriority(TicketPriority.Critical);

        // Assert - Recalculated 4 hours from ORIGINAL creation date, not current time
        var expectedDueDate = createdDate.AddHours(4);
        Assert.Equal(expectedDueDate, ticket.DueDate);
        Assert.Equal(TicketPriority.Critical, ticket.Priority);
    }
}