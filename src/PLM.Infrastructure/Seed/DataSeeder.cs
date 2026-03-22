using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PLM.Domain.Entities;
using PLM.Domain.Enums;
using PLM.Infrastructure.Data;
using System.Text.Json;

namespace PLM.Infrastructure.Seed;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PlmDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        await SeedRolesAsync(roleManager);
        await SeedUsersAsync(userManager);

        if (!await context.Products.AnyAsync())
        {
            await SeedProductsAndBoMsAsync(context);
        }
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = ["Admin", "Engineering", "Approver", "Operations"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        var users = new (string Email, string FullName, string Dept, string Role, string Password)[]
        {
            ("admin@plm.com",      "System Admin",     "IT",          "Admin",       "Admin@123"),
            ("engineer@plm.com",   "John Engineer",    "Engineering", "Engineering", "Eng@12345"),
            ("approver@plm.com",   "Jane Approver",    "Management",  "Approver",    "App@12345"),
            ("operations@plm.com", "Bob Operations",   "Operations",  "Operations",  "Ops@12345"),
        };

        foreach (var (email, fullName, dept, role, password) in users)
        {
            if (await userManager.FindByEmailAsync(email) == null)
            {
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName,
                    Department = dept,
                    EmailConfirmed = true,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(user, role);
            }
        }
    }

    private static async Task SeedProductsAndBoMsAsync(PlmDbContext context)
    {
        const int recordCount = 500;
        var now = DateTime.UtcNow;
        var rng = new Random(33);

        // ── Products (500) ───────────────────────────────────────
        var products = new List<Product>(recordCount);
        for (var i = 1; i <= recordCount; i++)
        {
            var createdAt = now.AddDays(-i);
            var versionNumber = i % 4 == 0 ? 2 : 1;
            var baseCost = 400m + (i * 9 % 1700);
            var salePrice = baseCost * 1.8m;

            var versions = new List<ProductVersion>
            {
                new()
                {
                    VersionNumber = 1,
                    CostPrice = baseCost,
                    SalePrice = salePrice,
                    IsActive = versionNumber == 1,
                    ChangeDescription = "Initial version",
                    CreatedAt = createdAt,
                    CreatedBy = "System"
                }
            };

            if (versionNumber == 2)
            {
                versions.Add(new ProductVersion
                {
                    VersionNumber = 2,
                    CostPrice = baseCost + 25,
                    SalePrice = salePrice + 65,
                    IsActive = true,
                    ChangeDescription = "Cost and sale price adjustment",
                    CreatedAt = createdAt.AddDays(2),
                    CreatedBy = "System"
                });
            }

            products.Add(new Product
            {
                Name = $"Demo Product {i:000}",
                Description = $"Demo seeded product record #{i} for UI, paging, filtering and workflow validation.",
                Status = i % 10 == 0 ? Status.Archived : Status.Active,
                CurrentVersionNumber = versionNumber,
                CreatedAt = createdAt,
                UpdatedAt = createdAt.AddDays(1),
                Versions = versions
            });
        }

        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        // ── BoMs (500) ────────────────────────────────────────────
        var boms = new List<BoM>(recordCount);
        for (var i = 1; i <= recordCount; i++)
        {
            var createdAt = now.AddDays(-i);
            var product = products[i - 1];

            boms.Add(new BoM
            {
                ProductId = product.Id,
                Name = $"Demo BoM {i:000}",
                Description = $"Seeded BoM for {product.Name}",
                Status = product.Status,
                CurrentVersionNumber = 1,
                CreatedAt = createdAt,
                UpdatedAt = createdAt.AddDays(1),
                Versions = new List<BoMVersion>
                {
                    new()
                    {
                        VersionNumber = 1,
                        IsActive = true,
                        ChangeDescription = "Initial BoM version",
                        CreatedAt = createdAt,
                        CreatedBy = "System",
                        Components = new List<BoMComponent>
                        {
                            new() { ComponentName = $"Base Component {i:000}-A", PartNumber = $"CMP-{i:000}-A", Quantity = 1 + i % 4, UnitCost = 20 + i % 60, Unit = "pcs" },
                            new() { ComponentName = $"Base Component {i:000}-B", PartNumber = $"CMP-{i:000}-B", Quantity = 2 + i % 5, UnitCost = 30 + i % 45, Unit = "pcs" },
                            new() { ComponentName = $"Base Component {i:000}-C", PartNumber = $"CMP-{i:000}-C", Quantity = 1 + i % 3, UnitCost = 15 + i % 35, Unit = "pcs" }
                        },
                        Operations = new List<BoMOperation>
                        {
                            new() { OperationName = "Assembly", Description = "Primary assembly", SequenceOrder = 10, EstimatedTime = 1.5m + (i % 3), WorkCenter = "ASM-01" },
                            new() { OperationName = "Inspection", Description = "Quality inspection", SequenceOrder = 20, EstimatedTime = 0.5m + (i % 2), WorkCenter = "QC-01" }
                        }
                    }
                }
            });
        }

        context.BoMs.AddRange(boms);
        await context.SaveChangesAsync();

        // ── ECOs (500) and Approvals ─────────────────────────────
        var ecos = new List<ECO>(recordCount);
        var approvals = new List<ECOApproval>();

        for (var i = 1; i <= recordCount; i++)
        {
            var stage = (i % 4) switch
            {
                0 => ECOStage.Done,
                1 => ECOStage.New,
                2 => ECOStage.Approval,
                _ => ECOStage.Rejected
            };

            var type = i % 2 == 0 ? ECOType.BoM : ECOType.Product;
            var createdAt = now.AddHours(-i * 6);

            var eco = new ECO
            {
                Title = $"Demo ECO {i:000}",
                Description = $"Seeded ECO record #{i} for stage/type coverage and approval workflows.",
                Type = type,
                Stage = stage,
                ProductId = products[(i - 1) % products.Count].Id,
                BoMId = type == ECOType.BoM ? boms[(i - 1) % boms.Count].Id : null,
                EffectiveDate = now.AddDays((i % 45) + 1),
                CreateNewVersion = i % 3 != 0,
                ProposedCostPrice = type == ECOType.Product ? 450m + (i % 1200) : null,
                ProposedSalePrice = type == ECOType.Product ? 900m + (i % 1800) : null,
                ProposedComponents = type == ECOType.BoM
                    ? JsonSerializer.Serialize(new[]
                    {
                        new { ComponentName = $"Proposed Comp {i:000}-A", PartNumber = $"PC-{i:000}-A", Quantity = 2 + (i % 3), UnitCost = 25 + (i % 20), Unit = "pcs" },
                        new { ComponentName = $"Proposed Comp {i:000}-B", PartNumber = $"PC-{i:000}-B", Quantity = 1 + (i % 4), UnitCost = 30 + (i % 25), Unit = "pcs" }
                    })
                    : null,
                ProposedOperations = type == ECOType.BoM
                    ? JsonSerializer.Serialize(new[]
                    {
                        new { OperationName = "Assembly", Description = "Updated routing", SequenceOrder = 10, EstimatedTime = 2.0m, WorkCenter = "ASM-01" },
                        new { OperationName = "Inspection", Description = "Final QC", SequenceOrder = 20, EstimatedTime = 1.0m, WorkCenter = "QC-01" }
                    })
                    : null,
                CreatedBy = "System User",
                CreatedAt = createdAt,
                UpdatedAt = createdAt.AddHours(2)
            };

            ecos.Add(eco);
        }

        context.ECOs.AddRange(ecos);
        await context.SaveChangesAsync();

        foreach (var eco in ecos.Where(e => e.Stage is ECOStage.Done or ECOStage.Rejected))
        {
            approvals.Add(new ECOApproval
            {
                ECOId = eco.Id,
                ApproverId = "approver@plm.com",
                ApproverName = "Jane Approver",
                Decision = eco.Stage == ECOStage.Done ? ApprovalDecision.Approved : ApprovalDecision.Rejected,
                Comments = eco.Stage == ECOStage.Done ? "Approved in seed data" : "Rejected in seed data",
                ApprovedAt = eco.UpdatedAt,
                CreatedAt = eco.UpdatedAt
            });
        }

        context.ECOApprovals.AddRange(approvals);
        await context.SaveChangesAsync();

        // ── Audit Logs (500) ──────────────────────────────────────
        var auditLogs = new List<AuditLog>(recordCount);
        for (var i = 1; i <= recordCount; i++)
        {
            var actionType = i % 5;
            var entityType = actionType switch
            {
                0 => "ECO",
                1 => "Product",
                2 => "BoM",
                3 => "ECO",
                _ => "Product"
            };

            var action = actionType switch
            {
                0 => "ECO Created",
                1 => "Product Created",
                2 => "BoM Created",
                3 => "ECO Approved",
                _ => "Product Updated"
            };

            var entityId = entityType switch
            {
                "Product" => products[(i - 1) % products.Count].Id,
                "BoM" => boms[(i - 1) % boms.Count].Id,
                _ => ecos[(i - 1) % ecos.Count].Id
            };

            auditLogs.Add(new AuditLog
            {
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                OldValue = action.Contains("Updated") ? $"Old value {i}" : null,
                NewValue = $"Seed event {i}",
                UserId = "system",
                UserName = i % 2 == 0 ? "System" : "Jane Approver",
                Timestamp = now.AddMinutes(-(i * 11 + rng.Next(0, 7)))
            });
        }

        context.AuditLogs.AddRange(auditLogs);
        await context.SaveChangesAsync();
    }
}
