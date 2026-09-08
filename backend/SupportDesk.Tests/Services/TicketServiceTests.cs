using Microsoft.EntityFrameworkCore;
using SupportDesk.Api.Services;
using SupportDesk.Core.Dtos;
using SupportDesk.Core.Entities;
using SupportDesk.Core.Enums;
using SupportDesk.Core.Exceptions;
using SupportDesk.Infrastructure.Data;
using Xunit;

namespace SupportDesk.Tests.Services;

public class TicketServiceTests
{
    private static AppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task AssignAgentAsync_InactiveAgent_ThrowsBusinessRuleException()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var inactiveAgent = new Agent
        {
            Id = 1,
            FullName = "Michael Scott",
            Email = "michael@support.com",
            Department = Department.General,
            Active = false // Inactive agent
        };

        var ticket = new Ticket
        {
            Id = 1,
            Reference = "TCK-2026-0001",
            Title = "Test Ticket",
            Description = "Test Desc",
            CustomerName = "John",
            CustomerEmail = "john@example.com"
        };

        context.Agents.Add(inactiveAgent);
        context.Tickets.Add(ticket);
        await context.SaveChangesAsync();

        var service = new TicketService(context);

        // Act & Assert (Rule #4: Inactive agent assignment fails)
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.AssignAgentAsync(ticket.Id, new AssignAgentDto(inactiveAgent.Id)));

        Assert.Equal("INACTIVE_AGENT", exception.Code);
    }
}