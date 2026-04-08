using Microsoft.EntityFrameworkCore;

namespace ForgePLM.Runtime.Data
{
    public class ForgePlmDbContext : DbContext
    {
        public ForgePlmDbContext(DbContextOptions<ForgePlmDbContext> options)
            : base(options)
        {
        }
    }
}