using Microsoft.EntityFrameworkCore;
using SupportDesk.Core.Entities;
using SupportDesk.Core.Enums;

namespace SupportDesk.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await context.Database.MigrateAsync();

        if (await context.Agents.AnyAsync())
        {
            return; // Data already seeded
        }

        // 1. Seed 5 Agents (4 active, 1 inactive for testing business rule #4)
        var agents = new List<Agent>
        {
            new() { FullName = "Sarah Connor", Email = "sarah.c@supportdesk.com", Department = Department.Technical, Active = true },
            new() { FullName = "John Doe", Email = "john.d@supportdesk.com", Department = Department.Billing, Active = true },
            new() { FullName = "Alex Mercer", Email = "alex.m@supportdesk.com", Department = Department.Technical, Active = true },
            new() { FullName = "Emily Blunt", Email = "emily.b@supportdesk.com", Department = Department.General, Active = true },
            new() { FullName = "Michael Scott", Email = "michael.s@supportdesk.com", Department = Department.General, Active = false } // Inactive
        };

        await context.Agents.AddRangeAsync(agents);
        await context.SaveChangesAsync();

        // 2. Seed 20 Tickets with varying dates, priorities, statuses
        var now = DateTime.UtcNow;

        var tickets = new List<Ticket>();

        // Helper to construct seeded tickets with reflective internal state
        Ticket CreateSeededTicket(
            int refNumber,
            string title,
            string desc,
            string customerName,
            string customerEmail,
            TicketPriority priority,
            TicketStatus status,
            Agent? assignedAgent,
            DateTime createdDate,
            DateTime? resolvedDate = null,
            DateTime? closedDate = null)
        {
            var ticket = new Ticket
            {
                Reference = $"TCK-2026-{refNumber:D4}",
                Title = title,
                Description = desc,
                CustomerName = customerName,
                CustomerEmail = customerEmail,
                CreatedDate = createdDate,
                LastModifiedDate = createdDate
            };

            ticket.Initialize(priority);

            if (assignedAgent != null)
            {
                ticket.AssignAgent(assignedAgent);
            }

            if (status != TicketStatus.New)
            {
                // Force transition for seeding purposes
                if (status == TicketStatus.InProgress)
                {
                    ticket.TransitionStatus(TicketStatus.InProgress);
                }
                else if (status == TicketStatus.Resolved)
                {
                    ticket.TransitionStatus(TicketStatus.InProgress);
                    ticket.TransitionStatus(TicketStatus.Resolved);
                }
                else if (status == TicketStatus.Closed)
                {
                    ticket.TransitionStatus(TicketStatus.InProgress);
                    ticket.TransitionStatus(TicketStatus.Resolved);
                    ticket.TransitionStatus(TicketStatus.Closed);
                }
            }

            return ticket;
        }

        // Overdue Tickets (Created days/hours ago)
        tickets.Add(CreateSeededTicket(1, "Critical Production Server Outage", "Database cluster unreachable in East region.", "TechCorp Inc.", "admin@techcorp.com", TicketPriority.Critical, TicketStatus.InProgress, agents[0], now.AddHours(-10))); // Overdue (>4 hrs)
        tickets.Add(CreateSeededTicket(2, "Billing Error on Renewal", "Double charged for yearly subscription.", "Alice Smith", "alice@gmail.com", TicketPriority.High, TicketStatus.New, null, now.AddDays(-3))); // Overdue (>1 day)
        tickets.Add(CreateSeededTicket(3, "API Integration Timeouts", "Webhook response latency exceeds 5000ms.", "DevStudio Ltd", "api@devstudio.io", TicketPriority.Normal, TicketStatus.InProgress, agents[2], now.AddDays(-5))); // Overdue (>3 days)

        // Normal/Active Tickets
        tickets.Add(CreateSeededTicket(4, "Cannot Reset Password", "Self-service password reset link throws 500 error.", "Bob Jones", "bob@example.com", TicketPriority.Low, TicketStatus.New, null, now.AddDays(-1)));
        tickets.Add(CreateSeededTicket(5, "Feature Request: Export to CSV", "Need ability to export table reports to CSV format.", "Carol White", "carol@enterprise.com", TicketPriority.Low, TicketStatus.InProgress, agents[3], now.AddHours(-12)));
        tickets.Add(CreateSeededTicket(6, "SSL Certificate Expiry Warning", "Wildcard cert expires in 3 days.", "CloudSystems", "ops@cloudsystems.net", TicketPriority.Critical, TicketStatus.InProgress, agents[0], now.AddHours(-1)));
        tickets.Add(CreateSeededTicket(7, "Incorrect Tax Calculation", "Invoice #8921 shows incorrect VAT rate.", "GlobalRetail", "finance@globalretail.com", TicketPriority.High, TicketStatus.InProgress, agents[1], now.AddHours(-6)));
        tickets.Add(CreateSeededTicket(8, "UI Glitch in Dark Mode", "Sidebar text becomes invisible on dark theme.", "David Brown", "david@web.com", TicketPriority.Low, TicketStatus.New, null, now.AddHours(-2)));
        tickets.Add(CreateSeededTicket(9, "Webhook Delivery Failure", "Events not reaching target endpoint.", "SaaS Dynamics", "integrations@saasdyn.com", TicketPriority.Normal, TicketStatus.InProgress, agents[2], now.AddHours(-18)));
        tickets.Add(CreateSeededTicket(10, "Account Suspension Inquiry", "Account locked after 3 failed password attempts.", "Eva Green", "eva@business.org", TicketPriority.Normal, TicketStatus.New, null, now.AddHours(-4)));

        // Resolved & Closed Tickets
        tickets.Add(CreateSeededTicket(11, "Domain Migration Assistance", "Need DNS records updated for new domain.", "Frank Wright", "frank@startup.co", TicketPriority.Normal, TicketStatus.Resolved, agents[0], now.AddDays(-4)));
        tickets.Add(CreateSeededTicket(12, "Refund Request for Unused Month", "Subscription cancelled before cycle start.", "Grace Hopper", "grace@tech.edu", TicketPriority.High, TicketStatus.Resolved, agents[1], now.AddDays(-2)));
        tickets.Add(CreateSeededTicket(13, "2FA Device Lost", "Requesting backup recovery keys.", "Henry Cavill", "henry@actor.com", TicketPriority.Critical, TicketStatus.Closed, agents[3], now.AddDays(-10)));
        tickets.Add(CreateSeededTicket(14, "SAML SSO Setup Guidance", "Configuring Okta identity provider.", "Enterprise Corp", "it@enterprisecorp.com", TicketPriority.Normal, TicketStatus.Closed, agents[2], now.AddDays(-8)));
        tickets.Add(CreateSeededTicket(15, "Broken Link in Welcome Email", "Verification link gives 404.", "Isla Fisher", "isla@gmail.com", TicketPriority.Low, TicketStatus.Closed, agents[3], now.AddDays(-12)));

        // Additional Variety
        tickets.Add(CreateSeededTicket(16, "Database CPU Spike", "PostgreSQL CPU hitting 100%.", "DataMetrics", "alerts@datametrics.com", TicketPriority.Critical, TicketStatus.InProgress, agents[0], now.AddHours(-2)));
        tickets.Add(CreateSeededTicket(17, "Update Credit Card Details", "Updated payment method failing.", "Jack Ryan", "jack@security.org", TicketPriority.High, TicketStatus.New, null, now.AddHours(-5)));
        tickets.Add(CreateSeededTicket(18, "Slow Query Logs Analysis", "Intermittent slow queries on reporting DB.", "FinTech Ltd", "dev@fintech.io", TicketPriority.Normal, TicketStatus.InProgress, agents[2], now.AddDays(-1)));
        tickets.Add(CreateSeededTicket(19, "Email Notifications Not Sending", "SMTP queue backed up.", "Logistics Inc", "support@logistics.com", TicketPriority.High, TicketStatus.Resolved, agents[0], now.AddDays(-3)));
        tickets.Add(CreateSeededTicket(20, "Mobile App Crash on Launch", "iOS app crashes on v2.4 launch.", "Karen Gillan", "karen@mobile.io", TicketPriority.Critical, TicketStatus.Resolved, agents[2], now.AddDays(-1)));

        await context.Tickets.AddRangeAsync(tickets);
        await context.SaveChangesAsync();

        // 3. Seed Comments for a few tickets
        var comments = new List<Comment>
        {
            new() { TicketId = tickets[0].Id, AuthorName = "Sarah Connor", Body = "Investigating connection timeouts on DB cluster.", CreatedDate = now.AddHours(-8) },
            new() { TicketId = tickets[0].Id, AuthorName = "TechCorp Inc.", Body = "Any updates? Systems are completely down.", CreatedDate = now.AddHours(-6) },
            new() { TicketId = tickets[10].Id, AuthorName = "Sarah Connor", Body = "DNS record updates complete and verified.", CreatedDate = now.AddDays(-3) }
        };

        await context.Comments.AddRangeAsync(comments);
        await context.SaveChangesAsync();
    }
}