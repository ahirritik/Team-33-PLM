using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PLM.Domain.Entities;
using PLM.Domain.Enums;
using PLM.Infrastructure.Data;

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
        // ── Products ─────────────────────────────────────────────
        var products = new[]
        {
            new Product
            {
                Name = "Hydraulic Pump Assembly",
                Description = "High-pressure hydraulic pump for industrial machinery",
                Status = Status.Active,
                CurrentVersionNumber = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Versions = new List<ProductVersion>
                {
                    new() { VersionNumber = 1, CostPrice = 1250.00m, SalePrice = 2499.99m, IsActive = true, ChangeDescription = "Initial version", CreatedAt = DateTime.UtcNow, CreatedBy = "System" }
                }
            },
            new Product
            {
                Name = "Electric Motor Drive",
                Description = "Variable frequency drive for electric motors",
                Status = Status.Active,
                CurrentVersionNumber = 2,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow,
                Versions = new List<ProductVersion>
                {
                    new() { VersionNumber = 1, CostPrice = 800.00m, SalePrice = 1599.99m, IsActive = false, ChangeDescription = "Initial version", CreatedAt = DateTime.UtcNow.AddDays(-30), CreatedBy = "System" },
                    new() { VersionNumber = 2, CostPrice = 850.00m, SalePrice = 1699.99m, IsActive = true, ChangeDescription = "Upgraded capacitors", CreatedAt = DateTime.UtcNow, CreatedBy = "System" }
                }
            },
            new Product
            {
                Name = "Control Panel Unit",
                Description = "PLC-based control panel for automation systems",
                Status = Status.Active,
                CurrentVersionNumber = 1,
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                UpdatedAt = DateTime.UtcNow.AddDays(-15),
                Versions = new List<ProductVersion>
                {
                    new() { VersionNumber = 1, CostPrice = 3200.00m, SalePrice = 5999.99m, IsActive = true, ChangeDescription = "Initial version", CreatedAt = DateTime.UtcNow.AddDays(-15), CreatedBy = "System" }
                }
            }
        };

        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        // ── BoMs ─────────────────────────────────────────────────
        var bom1 = new BoM
        {
            ProductId = products[0].Id,
            Name = "Hydraulic Pump BoM",
            Description = "Bill of Materials for Hydraulic Pump Assembly",
            Status = Status.Active,
            CurrentVersionNumber = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Versions = new List<BoMVersion>
            {
                new()
                {
                    VersionNumber = 1,
                    IsActive = true,
                    ChangeDescription = "Initial version",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System",
                    Components = new List<BoMComponent>
                    {
                        new() { ComponentName = "Pump Housing", PartNumber = "PH-001", Quantity = 1, UnitCost = 350.00m, Unit = "pcs" },
                        new() { ComponentName = "Piston Set",   PartNumber = "PS-002", Quantity = 4, UnitCost = 75.00m,  Unit = "pcs" },
                        new() { ComponentName = "Seal Kit",     PartNumber = "SK-003", Quantity = 2, UnitCost = 25.00m,  Unit = "set" },
                        new() { ComponentName = "Drive Shaft",  PartNumber = "DS-004", Quantity = 1, UnitCost = 180.00m, Unit = "pcs" },
                        new() { ComponentName = "Bearing Set",  PartNumber = "BS-005", Quantity = 2, UnitCost = 45.00m,  Unit = "set" }
                    },
                    Operations = new List<BoMOperation>
                    {
                        new() { OperationName = "CNC Machining",    Description = "Machine pump housing", SequenceOrder = 1, EstimatedTime = 4.5m, WorkCenter = "CNC-01" },
                        new() { OperationName = "Assembly",         Description = "Assemble pump components", SequenceOrder = 2, EstimatedTime = 2.0m, WorkCenter = "ASM-01" },
                        new() { OperationName = "Quality Testing",  Description = "Pressure and leak testing", SequenceOrder = 3, EstimatedTime = 1.0m, WorkCenter = "QC-01" }
                    }
                }
            }
        };

        var bom2 = new BoM
        {
            ProductId = products[1].Id,
            Name = "Motor Drive BoM",
            Description = "Bill of Materials for Electric Motor Drive",
            Status = Status.Active,
            CurrentVersionNumber = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Versions = new List<BoMVersion>
            {
                new()
                {
                    VersionNumber = 1,
                    IsActive = true,
                    ChangeDescription = "Initial version",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System",
                    Components = new List<BoMComponent>
                    {
                        new() { ComponentName = "PCB Board",        PartNumber = "PCB-101", Quantity = 1, UnitCost = 120.00m, Unit = "pcs" },
                        new() { ComponentName = "IGBT Module",      PartNumber = "IG-102",  Quantity = 6, UnitCost = 45.00m,  Unit = "pcs" },
                        new() { ComponentName = "Capacitor Bank",   PartNumber = "CB-103",  Quantity = 4, UnitCost = 30.00m,  Unit = "pcs" },
                        new() { ComponentName = "Heat Sink",        PartNumber = "HS-104",  Quantity = 1, UnitCost = 65.00m,  Unit = "pcs" },
                        new() { ComponentName = "Enclosure",        PartNumber = "EN-105",  Quantity = 1, UnitCost = 85.00m,  Unit = "pcs" }
                    },
                    Operations = new List<BoMOperation>
                    {
                        new() { OperationName = "SMD Soldering",    Description = "Surface mount assembly", SequenceOrder = 1, EstimatedTime = 1.5m, WorkCenter = "SMD-01" },
                        new() { OperationName = "THT Assembly",     Description = "Through-hole component placement", SequenceOrder = 2, EstimatedTime = 1.0m, WorkCenter = "THT-01" },
                        new() { OperationName = "Final Assembly",   Description = "Board mounting and wiring", SequenceOrder = 3, EstimatedTime = 2.0m, WorkCenter = "ASM-02" },
                        new() { OperationName = "Burn-in Test",     Description = "48-hour burn-in testing", SequenceOrder = 4, EstimatedTime = 48.0m, WorkCenter = "QC-02" }
                    }
                }
            }
        };

        context.BoMs.AddRange(bom1, bom2);
        await context.SaveChangesAsync();

        // ── Sample Audit Logs ────────────────────────────────────
        var auditLogs = new[]
        {
            new AuditLog { Action = "Product Created",  EntityType = "Product", EntityId = products[0].Id, NewValue = "Hydraulic Pump Assembly", UserId = "system", UserName = "System", Timestamp = DateTime.UtcNow.AddDays(-30) },
            new AuditLog { Action = "Product Created",  EntityType = "Product", EntityId = products[1].Id, NewValue = "Electric Motor Drive",   UserId = "system", UserName = "System", Timestamp = DateTime.UtcNow.AddDays(-30) },
            new AuditLog { Action = "Product Created",  EntityType = "Product", EntityId = products[2].Id, NewValue = "Control Panel Unit",     UserId = "system", UserName = "System", Timestamp = DateTime.UtcNow.AddDays(-15) },
            new AuditLog { Action = "BoM Created",      EntityType = "BoM",     EntityId = bom1.Id,        NewValue = "Hydraulic Pump BoM",     UserId = "system", UserName = "System", Timestamp = DateTime.UtcNow },
            new AuditLog { Action = "BoM Created",      EntityType = "BoM",     EntityId = bom2.Id,        NewValue = "Motor Drive BoM",        UserId = "system", UserName = "System", Timestamp = DateTime.UtcNow },
        };

        context.AuditLogs.AddRange(auditLogs);
        await context.SaveChangesAsync();
    }
}
