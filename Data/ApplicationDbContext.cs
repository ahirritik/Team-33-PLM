using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PLM.Models;

namespace PLM.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<BoM> BoMs { get; set; }
    public DbSet<BoMComponent> BoMComponents { get; set; }
    public DbSet<BoMOperation> BoMOperations { get; set; }
    public DbSet<ECO> ECOs { get; set; }
    public DbSet<ECOLog> ECOLogs { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasSequence<int>("ECONumberSeq", schema: "dbo")
            .StartsAt(1)
            .IncrementsBy(1);

        builder.Entity<ECO>()
            .Property(e => e.ReferenceNumber)
            .HasDefaultValueSql("CONCAT('ECO-', FORMAT(NEXT VALUE FOR dbo.ECONumberSeq, '000000'))");

        builder.Entity<ECO>()
            .Property(e => e.RowVersion)
            .IsRowVersion();

        builder.Entity<BoMComponent>()
            .HasOne(bc => bc.BoM)
            .WithMany(b => b.Components)
            .HasForeignKey(bc => bc.BoMId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.Entity<BoMOperation>()
            .HasOne(bo => bo.BoM)
            .WithMany(b => b.Operations)
            .HasForeignKey(bo => bo.BoMId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<BoMComponent>()
            .HasOne(bc => bc.Product)
            .WithMany()
            .HasForeignKey(bc => bc.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.Entity<BoM>()
            .HasOne(b => b.Product)
            .WithMany(p => p.BoMs)
            .HasForeignKey(b => b.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // AuditLog Configuration
        builder.Entity<AuditLog>()
            .Property(a => a.Id)
            .ValueGeneratedOnAdd();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = new List<AuditLog>();
        var entries = ChangeTracker.Entries().Where(e => e.Entity is not AuditLog && e.State != EntityState.Detached && e.State != EntityState.Unchanged).ToList();

        foreach (var entry in entries)
        {
            var auditLog = new AuditLog
            {
                EntityName = entry.Entity.GetType().Name,
                Action = entry.State.ToString(),
                Timestamp = DateTime.UtcNow,
                UserId = "System"
            };

            var primaryKey = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
            if (primaryKey != null && primaryKey.CurrentValue != null)
            {
                auditLog.RecordId = primaryKey.CurrentValue.ToString() ?? "";
            }

            if (entry.State == EntityState.Modified)
            {
                var oldValues = new Dictionary<string, object?>();
                var newValues = new Dictionary<string, object?>();
                foreach (var property in entry.Properties)
                {
                    if (property.IsModified)
                    {
                        oldValues[property.Metadata.Name] = property.OriginalValue;
                        newValues[property.Metadata.Name] = property.CurrentValue;
                    }
                }
                auditLog.OldValues = System.Text.Json.JsonSerializer.Serialize(oldValues);
                auditLog.NewValues = System.Text.Json.JsonSerializer.Serialize(newValues);
            }
            else if (entry.State == EntityState.Added)
            {
                var newValues = new Dictionary<string, object?>();
                foreach (var property in entry.Properties)
                {
                    newValues[property.Metadata.Name] = property.CurrentValue;
                }
                auditLog.NewValues = System.Text.Json.JsonSerializer.Serialize(newValues);
            }
            else if (entry.State == EntityState.Deleted)
            {
                var oldValues = new Dictionary<string, object?>();
                foreach (var property in entry.Properties)
                {
                    oldValues[property.Metadata.Name] = property.OriginalValue;
                }
                auditLog.OldValues = System.Text.Json.JsonSerializer.Serialize(oldValues);
            }
            auditEntries.Add(auditLog);
        }

        if (auditEntries.Any())
        {
            AuditLogs.AddRange(auditEntries);
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
