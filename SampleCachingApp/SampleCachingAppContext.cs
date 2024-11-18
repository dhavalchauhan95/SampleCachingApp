using Microsoft.EntityFrameworkCore;

namespace SampleCachingApp
{
    public class SampleCachingAppContext : DbContext
    {
        public SampleCachingAppContext(DbContextOptions<SampleCachingAppContext> options) : base(options)
        {
        }

        public DbSet<Employee> Employee { get; set; }
    }
}
