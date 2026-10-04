using Microsoft.EntityFrameworkCore;
using JyanTrackAPI.Models;

namespace JyanTrackAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Player> Players => Set<Player>();
    public DbSet<MatchRecord> MatchRecords => Set<MatchRecord>();
    public DbSet<MatchPlacement> MatchPlacements => Set<MatchPlacement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Ensure external match ID is unique
        modelBuilder.Entity<MatchRecord>()
            .HasIndex(m => m.ExternalId)
            .IsUnique();
        
        //Ensure player ID is unique
        modelBuilder.Entity<Player>()
            .HasIndex(p => p.AccountId)
            .IsUnique();

        // Configure 1-to-many relationship between MatchRecord and MatchPlacement
        modelBuilder.Entity<MatchPlacement>()
            .HasOne(mp => mp.MatchRecord)
            .WithMany(mr => mr.Placements)
            .HasForeignKey(mp => mp.MatchRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure 1-to-many relationship between Player and MatchPlacement
        modelBuilder.Entity<MatchPlacement>()
            .HasOne(mp => mp.Player)
            .WithMany(p => p.Placements)
            .HasForeignKey(mp => mp.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}