using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportDesk.Core.Entities;

namespace SupportDesk.Infrastructure.Data.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.HasKey(t => t.Id);

        // Requirement: Reference must be auto-generated format, unique
        builder.Property(t => t.Reference)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(t => t.Reference)
            .IsUnique();

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .IsRequired();

        builder.Property(t => t.CustomerName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.CustomerEmail)
            .IsRequired()
            .HasMaxLength(150);

        // Store Enums as Strings in DB for human readability
        builder.Property(t => t.Priority)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // Foreign Key Relationship: Agent (0..1) -> Tickets (Many)
        builder.HasOne(t => t.AssignedAgent)
            .WithMany(a => a.AssignedTickets)
            .HasForeignKey(t => t.AssignedAgentId)
            .OnDelete(DeleteBehavior.SetNull);

        // Ignore computed property on EF level
        builder.Ignore(t => t.IsOverdue);
    }
}