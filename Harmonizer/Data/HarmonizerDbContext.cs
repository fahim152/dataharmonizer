using Microsoft.EntityFrameworkCore;
using Harmonizer.Models;

namespace Harmonizer.Data;

public class HarmonizerDbContext : DbContext
{
    public HarmonizerDbContext(DbContextOptions<HarmonizerDbContext> options) : base(options)
    {
    }

    public DbSet<HarmonizedRecord> HarmonizedRecords => Set<HarmonizedRecord>();
    public DbSet<HarmonizeRequest> HarmonizeRequests => Set<HarmonizeRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HarmonizedRecord>(entity =>
        {
            entity.ToTable("HarmonizedRecords");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RawJson).IsRequired();
            entity.Property(e => e.ReceivedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.HarmonizedJson).HasColumnName("HarmonizedJson");
            entity.HasOne(e => e.HarmonizeRequest)
                .WithMany(r => r.HarmonizedRecords)
                .HasForeignKey(e => e.HarmonizeRequestId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<HarmonizeRequest>(entity =>
        {
            entity.ToTable("HarmonizeRequests");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Payload).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}
