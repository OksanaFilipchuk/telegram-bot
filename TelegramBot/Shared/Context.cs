using Microsoft.EntityFrameworkCore;
using TelegramBot.Model;

namespace TelegramBot.Shared;

public class Context : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=MyDb;Trusted_Connection=True");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExpensesCategory>()
            .HasIndex(c => c.Key)
            .IsUnique();
    }

    public DbSet<ExpensesCategory> ExpensesCategory { get; set; } = null!;
    public DbSet<Expenses> Expenses { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;

}
