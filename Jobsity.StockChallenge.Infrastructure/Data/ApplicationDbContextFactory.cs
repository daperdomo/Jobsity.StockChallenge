using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Jobsity.StockChallenge.Infrastructure.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("STOCK_CHALLENGE_CONNECTION")
                ?? "Server=(localdb)\\mssqllocaldb;Database=JobsityStockChallenge;Trusted_Connection=True;MultipleActiveResultSets=true";

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            return new ApplicationDbContext(options);
        }
    }
}
