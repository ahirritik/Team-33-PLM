using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PLM.Infrastructure.Data;

public class PlmDbContextFactory : IDesignTimeDbContextFactory<PlmDbContext>
{
    public PlmDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<PlmDbContext>();
        var connectionString = "Server=(localdb)\\mssqllocaldb;Database=PLM_Db;Trusted_Connection=True;MultipleActiveResultSets=true";

        builder.UseSqlServer(connectionString, b => b.MigrationsAssembly("PLM.Infrastructure"));

        return new PlmDbContext(builder.Options);
    }
}
