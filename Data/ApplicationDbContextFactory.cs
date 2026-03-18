using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO; // Para Directory

namespace Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Database=ClientesPDF;Username=postgres;Password=test");
            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}