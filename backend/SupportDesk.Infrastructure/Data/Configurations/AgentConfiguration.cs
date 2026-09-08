using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportDesk.Core.Entities;

namespace SupportDesk.Infrastructure.Data.Configurations;

public class AgentConfiguration : IEntityTypeConfiguration<Agent>
{
    public void Configure(EntityTypeBuilder<Agent> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.FullName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Email)
            .IsRequired()
            .HasMaxLength(150);

        // Requirement: Email must be unique
        builder.HasIndex(a => a.Email)
            .IsUnique();

        builder.Property(a => a.Department)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(a => a.Active)
            .IsRequired();
    }
}