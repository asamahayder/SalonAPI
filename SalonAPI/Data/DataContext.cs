using Microsoft.EntityFrameworkCore;
using SalonAPI.Models;

namespace SalonAPI.Data
{
    public class DataContext: DbContext
    {
        public DataContext(DbContextOptions<DataContext> options): base(options)
        {

        }

        public DbSet<SalonChain> SalonChains { get; set; }
    }
}
