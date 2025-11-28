using Microsoft.EntityFrameworkCore;
using Shop.Domain.Entities;

namespace Shop.Infrastructure;

internal sealed class ApplicationContext : DbContext
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Item> Items => Set<Item>();
    
    public DbSet<Purchase> Purchases => Set<Purchase>();

    public ApplicationContext(DbContextOptions<ApplicationContext> options)
        : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}
