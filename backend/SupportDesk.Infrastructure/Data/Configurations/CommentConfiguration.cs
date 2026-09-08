using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportDesk.Core.Entities;

namespace SupportDesk.Infrastructure.Data.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.AuthorName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Body)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(c => c.CreatedDate)
            .IsRequired();

        // Foreign Key Relationship: Ticket (1) -> Comments (Many)
        builder.HasOne(c => c.Ticket)
            .WithMany(t => t.Comments)
            .HasForeignKey(c => c.TicketId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}