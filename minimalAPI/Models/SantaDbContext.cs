using minimalAPI.Models;
using Microsoft.EntityFrameworkCore;

class SantaDbContext : DbContext
{
     public SantaDbContext(DbContextOptions<SantaDbContext> options)
            : base(options)
        {
        }

    public DbSet<Child> Children { get; set; } = null!;
    public DbSet<Gift> Gifts { get; set; } = null!;
}