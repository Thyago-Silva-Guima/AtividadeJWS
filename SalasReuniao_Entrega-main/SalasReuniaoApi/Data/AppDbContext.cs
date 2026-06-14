using Microsoft.EntityFrameworkCore;
using SalasReuniaoApi.Models;

namespace SalasReuniaoApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<SalaReuniao> SalasReuniao { get; set; }
    }
}
