using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PLM.Data;
using PLM.Models;

namespace PLM.Controllers
{
    [Authorize]
    public class BoMsController : Controller
    {
        private readonly string _connectionString;
        private readonly ApplicationDbContext _context;

        public BoMsController(IConfiguration configuration, ApplicationDbContext context)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
            _context = context;
        }

        // GET: BoMs
        public async Task<IActionResult> Index()
        {
            using var connection = new SqlConnection(_connectionString);
            var query = @"
                SELECT b.*, p.Name AS ProductName 
                FROM BoMs b
                JOIN Products p ON b.ProductId = p.Id
                ORDER BY b.Id DESC";
                
            var boms = await connection.QueryAsync<BoMViewModel>(query);
            return View(boms);
        }

        // GET: BoMs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            using var connection = new SqlConnection(_connectionString);
            
            // 1. Get BoM header
            var queryBoM = @"
                SELECT b.*, p.Name AS ProductName 
                FROM BoMs b
                JOIN Products p ON b.ProductId = p.Id
                WHERE b.Id = @Id";
            var bom = await connection.QueryFirstOrDefaultAsync<BoMViewModel>(queryBoM, new { Id = id });
            
            if (bom == null) return NotFound();

            // 2. Get Components
            var queryComponents = @"
                SELECT bc.*, p.Name as ProductName
                FROM BoMComponents bc
                JOIN Products p ON bc.ProductId = p.Id
                WHERE bc.BoMId = @Id";
            bom.ComponentsList = (await connection.QueryAsync<BoMComponentViewModel>(queryComponents, new { Id = id })).ToList();

            // 3. Get Operations
            var queryOperations = "SELECT * FROM BoMOperations WHERE BoMId = @Id";
            bom.OperationsList = (await connection.QueryAsync<BoMOperation>(queryOperations, new { Id = id })).ToList();

            return View(bom);
        }

        // GET: BoMs/Create
        [Authorize(Roles = "Admin,Engineering User")]
        public async Task<IActionResult> Create()
        {
            using var connection = new SqlConnection(_connectionString);
            var products = await connection.QueryAsync<Product>("SELECT * FROM Products WHERE Status = @Status", new { Status = (int)ProductStatus.Active });
            ViewBag.Products = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(products, "Id", "Name");
            return View();
        }

        // POST: BoMs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Engineering User")]
        public async Task<IActionResult> Create([Bind("ProductId")] BoM bom)
        {
            ModelState.Clear();
            if (bom.ProductId <= 0)
            {
                ModelState.AddModelError("ProductId", "Please select a valid product.");
            }

            if (ModelState.IsValid)
            {
                bom.Version = "v1";
                bom.Status = BoMStatus.Active;
                _context.Add(bom);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = bom.Id });
            }
            
            using var connection = new SqlConnection(_connectionString);
            var products = await connection.QueryAsync<Product>("SELECT * FROM Products WHERE Status = @Status", new { Status = (int)ProductStatus.Active });
            ViewBag.Products = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(products, "Id", "Name", bom.ProductId);
            
            return View(bom);
        }

        // POST: BoMs/AddComponent
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Engineering User")]
        public async Task<IActionResult> AddComponent(int BoMId, int ProductId, decimal Quantity)
        {
            var comp = new BoMComponent { BoMId = BoMId, ProductId = ProductId, Quantity = Quantity };
            _context.BoMComponents.Add(comp);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = BoMId });
        }

        // POST: BoMs/RemoveComponent
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Engineering User")]
        public async Task<IActionResult> RemoveComponent(int componentId, int bomId)
        {
            var comp = await _context.BoMComponents.FindAsync(componentId);
            if (comp != null) { _context.BoMComponents.Remove(comp); await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(Details), new { id = bomId });
        }

        // POST: BoMs/AddOperation
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Engineering User")]
        public async Task<IActionResult> AddOperation(int BoMId, string Name, string WorkCenter, int TimeMinutes)
        {
            var op = new BoMOperation { BoMId = BoMId, Name = Name, WorkCenter = WorkCenter, TimeMinutes = TimeMinutes };
            _context.BoMOperations.Add(op);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = BoMId });
        }

        // POST: BoMs/RemoveOperation
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Engineering User")]
        public async Task<IActionResult> RemoveOperation(int operationId, int bomId)
        {
            var op = await _context.BoMOperations.FindAsync(operationId);
            if (op != null) { _context.BoMOperations.Remove(op); await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(Details), new { id = bomId });
        }
    }

    public class BoMViewModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public BoMStatus Status { get; set; }
        public List<BoMComponentViewModel> ComponentsList { get; set; } = new();
        public List<BoMOperation> OperationsList { get; set; } = new();
    }

    public class BoMComponentViewModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
    }
}
