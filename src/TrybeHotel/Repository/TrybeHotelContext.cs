using Microsoft.EntityFrameworkCore;
using TrybeHotel.Models;

namespace TrybeHotel.Repository;
public class TrybeHotelContext : DbContext, ITrybeHotelContext
{
    public DbSet<Hotel> Hotels { get; set; }

    public DbSet<Room> Rooms { get; set; }

    public DbSet<City> Cities { get; set; }

    public TrybeHotelContext(DbContextOptions<TrybeHotelContext> options) : base(options) { }
    public TrybeHotelContext() { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(@"Server=127.0.0.1;Database=master;User=SA;Password=password123!;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Definição da relação com Author
        modelBuilder.Entity<Room>()
            .HasOne(c => c.Hotel)
            .WithMany(h => h.Rooms)
            .HasForeignKey(c => c.HotelId);

        // Definição da relação com Publisher
        modelBuilder.Entity<Hotel>()
            .HasOne(h => h.City)
            .WithMany(c => c.Hotels)
            .HasForeignKey(h => h.CityId);
    }
}