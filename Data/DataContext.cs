using Microsoft.EntityFrameworkCore;

namespace dotnet_rpg.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
        
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Host=localhost:5438;Database=dotnet-rpg;Username=postgres;Password=postgres");
    
    public DbSet<Character> Characters { get; set; }
    public DbSet<User> Users { get; set; }
}
