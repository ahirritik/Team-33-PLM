using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PLM.Domain.Entities;

namespace PLM.Infrastructure.Data;

public class PlmDbContext : IdentityDbContext<ApplicationUser>
{
    public PlmDbContext(DbContextOptions<PlmDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVersion> ProductVersions => Set<ProductVersion>();
    public DbSet<BoM> BoMs => Set<BoM>();
    public DbSet<BoMVersion> BoMVersions => Set<BoMVersion>();
    public DbSet<BoMComponent> BoMComponents => Set<BoMComponent>();
    public DbSet<BoMOperation> BoMOperations => Set<BoMOperation>();
    public DbSet<ECO> ECOs => Set<ECO>();
    public DbSet<ECOApproval> ECOApprovals => Set<ECOApproval>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ── Product ──────────────────────────────────────────────
        builder.Entity<Product>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Name).IsRequired().HasMaxLength(255);
            e.Property(p => p.Description).HasMaxLength(2000);
            e.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(p => p.Name);
            e.HasIndex(p => p.Status);
            e.HasQueryFilter(p => !p.IsDeleted);
        });

        // ── ProductVersion ───────────────────────────────────────
        builder.Entity<ProductVersion>(e =>
        {
            e.HasKey(v => v.Id);
            e.Property(v => v.CostPrice).HasColumnType("decimal(18,2)");
            e.Property(v => v.SalePrice).HasColumnType("decimal(18,2)");
            e.Property(v => v.ChangeDescription).HasMaxLength(500);
            e.Property(v => v.CreatedBy).HasMaxLength(100);
            e.Property(v => v.Attachments).HasMaxLength(2000);
            e.HasIndex(v => new { v.ProductId, v.VersionNumber }).IsUnique();
            e.HasIndex(v => v.IsActive);

            e.HasOne(v => v.Product)
                .WithMany(p => p.Versions)
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── BoM ──────────────────────────────────────────────────
        builder.Entity<BoM>(e =>
        {
            e.HasKey(b => b.Id);
            e.Property(b => b.Name).IsRequired().HasMaxLength(200);
            e.Property(b => b.Description).HasMaxLength(2000);
            e.Property(b => b.Status).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(b => b.ProductId);
            e.HasIndex(b => b.Status);
            e.HasQueryFilter(b => !b.IsDeleted);

            e.HasOne(b => b.Product)
                .WithMany(p => p.BoMs)
                .HasForeignKey(b => b.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── BoMVersion ───────────────────────────────────────────
        builder.Entity<BoMVersion>(e =>
        {
            e.HasKey(v => v.Id);
            e.Property(v => v.ChangeDescription).HasMaxLength(500);
            e.Property(v => v.CreatedBy).HasMaxLength(100);
            e.HasIndex(v => new { v.BoMId, v.VersionNumber }).IsUnique();
            e.HasIndex(v => v.IsActive);

            e.HasOne(v => v.BoM)
                .WithMany(b => b.Versions)
                .HasForeignKey(v => v.BoMId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── BoMComponent ─────────────────────────────────────────
        builder.Entity<BoMComponent>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.ComponentName).IsRequired().HasMaxLength(200);
            e.Property(c => c.PartNumber).HasMaxLength(100);
            e.Property(c => c.UnitCost).HasColumnType("decimal(18,2)");
            e.Property(c => c.Unit).HasMaxLength(20);
            e.HasIndex(c => c.BoMVersionId);

            e.HasOne(c => c.BoMVersion)
                .WithMany(v => v.Components)
                .HasForeignKey(c => c.BoMVersionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── BoMOperation ─────────────────────────────────────────
        builder.Entity<BoMOperation>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.OperationName).IsRequired().HasMaxLength(200);
            e.Property(o => o.Description).HasMaxLength(500);
            e.Property(o => o.WorkCenter).HasMaxLength(100);
            e.Property(o => o.EstimatedTime).HasColumnType("decimal(10,2)");
            e.HasIndex(o => o.BoMVersionId);

            e.HasOne(o => o.BoMVersion)
                .WithMany(v => v.Operations)
                .HasForeignKey(o => o.BoMVersionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── ECO ──────────────────────────────────────────────────
        builder.Entity<ECO>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);
            e.Property(x => x.Description).HasMaxLength(2000);
            e.Property(x => x.Type).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Stage).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.ProposedCostPrice).HasColumnType("decimal(18,2)");
            e.Property(x => x.ProposedSalePrice).HasColumnType("decimal(18,2)");
            e.Property(x => x.CreatedBy).HasMaxLength(100);
            e.HasIndex(x => x.Stage);
            e.HasIndex(x => x.Type);
            e.HasIndex(x => x.ProductId);
            e.HasIndex(x => x.CreatedAt);

            e.HasOne(x => x.Product)
                .WithMany(p => p.ECOs)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.BoM)
                .WithMany(b => b.ECOs)
                .HasForeignKey(x => x.BoMId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        });

        // ── ECOApproval ──────────────────────────────────────────
        builder.Entity<ECOApproval>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.ApproverId).IsRequired().HasMaxLength(450);
            e.Property(a => a.ApproverName).HasMaxLength(100);
            e.Property(a => a.Decision).HasConversion<string>().HasMaxLength(20);
            e.Property(a => a.Comments).HasMaxLength(2000);
            e.HasIndex(a => a.ECOId);
            e.HasIndex(a => a.Decision);

            e.HasOne(a => a.ECO)
                .WithMany(x => x.Approvals)
                .HasForeignKey(a => a.ECOId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── AuditLog ─────────────────────────────────────────────
        builder.Entity<AuditLog>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Action).IsRequired().HasMaxLength(100);
            e.Property(a => a.EntityType).IsRequired().HasMaxLength(100);
            e.Property(a => a.UserId).HasMaxLength(450);
            e.Property(a => a.UserName).HasMaxLength(100);
            e.HasIndex(a => new { a.EntityType, a.EntityId });
            e.HasIndex(a => a.Timestamp);
            e.HasIndex(a => a.UserId);
        });
    }
}
