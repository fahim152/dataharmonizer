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
    public DbSet<Prompt> Prompts => Set<Prompt>();
    public DbSet<Workflow> Workflows => Set<Workflow>();
    
    // Multi-tenancy
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Namespace> Namespaces => Set<Namespace>();
    public DbSet<Team> Teams => Set<Team>();
    
    // Workflow configuration
    public DbSet<InputConfig> InputConfigs => Set<InputConfig>();
    public DbSet<OutputConfig> OutputConfigs => Set<OutputConfig>();

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
            entity.HasOne(e => e.Workflow)
                .WithMany(w => w.HarmonizeRequests)
                .HasForeignKey(e => e.WorkflowId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Prompt>(entity =>
        {
            entity.ToTable("Prompts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ContextPrompt).IsRequired();
            entity.Property(e => e.DesiredOutput).IsRequired();
            entity.Property(e => e.DesiredOutputType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<Workflow>(entity =>
        {
            entity.ToTable("Workflows");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ContextPrompt).IsRequired();
            entity.Property(e => e.DesiredOutput).IsRequired();
            entity.Property(e => e.DesiredOutputType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ApiKey).IsRequired().HasMaxLength(64);
            entity.HasIndex(e => e.ApiKey).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasOne(e => e.Team)
                .WithMany(t => t.Workflows)
                .HasForeignKey(e => e.TeamId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.InputConfig)
                .WithOne(i => i.Workflow)
                .HasForeignKey<InputConfig>(i => i.WorkflowId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.OutputConfig)
                .WithOne(o => o.Workflow)
                .HasForeignKey<OutputConfig>(o => o.WorkflowId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Multi-tenancy configuration
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable("Tenants");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<Namespace>(entity =>
        {
            entity.ToTable("Namespaces");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.Namespaces)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.TenantId, e.Slug }).IsUnique();
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.ToTable("Teams");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasOne(e => e.Namespace)
                .WithMany(n => n.Teams)
                .HasForeignKey(e => e.NamespaceId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.NamespaceId, e.Slug }).IsUnique();
        });

        // Workflow config
        modelBuilder.Entity<InputConfig>(entity =>
        {
            entity.ToTable("InputConfigs");
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<OutputConfig>(entity =>
        {
            entity.ToTable("OutputConfigs");
            entity.HasKey(e => e.Id);
        });
    }
}
