using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SupportDesk.Infrastructure.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        
        // Fallback local connection string used specifically for design-time tooling (migrations)
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=SupportDeskDb;Trusted_Connection=True;MultipleActiveResultSets=true");

        return new AppDbContext(optionsBuilder.Options);
    }
}