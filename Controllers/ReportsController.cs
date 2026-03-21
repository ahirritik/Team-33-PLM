using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PLM.Models;

namespace PLM.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly string _connectionString;

        public ReportsController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        // GET: Reports (Dashboard)
        public async Task<IActionResult> Index()
        {
            using var connection = new SqlConnection(_connectionString);
            
            // Generate some top-level metrics for reporting using Dapper
            var metrics = await connection.QueryFirstOrDefaultAsync<dynamic>(@"
                SELECT 
                    (SELECT COUNT(*) FROM ECOs WHERE Stage = 0) AS DraftECOs,
                    (SELECT COUNT(*) FROM ECOs WHERE Stage = 1) AS PendingECOs,
                    (SELECT COUNT(*) FROM Products WHERE Status = 0) AS ActiveProducts,
                    (SELECT COUNT(*) FROM BoMs WHERE Status = 0) AS ActiveBoMs
            ") ?? new { DraftECOs = 0, PendingECOs = 0, ActiveProducts = 0, ActiveBoMs = 0 };

            ViewBag.Metrics = metrics;

            return View();
        }

        // GET: Reports/AuditLogs
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AuditLogs()
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "SELECT TOP 100 * FROM AuditLogs ORDER BY Id DESC";
            var logs = await connection.QueryAsync<AuditLog>(query);
            return View(logs);
        }
    }
}
