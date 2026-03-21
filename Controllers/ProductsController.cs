using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PLM.Data;
using PLM.Models;

namespace PLM.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly string _connectionString;
        private readonly ApplicationDbContext _context;

        public ProductsController(IConfiguration configuration, ApplicationDbContext context)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "SELECT * FROM Products ORDER BY Id DESC";
            var products = await connection.QueryAsync<Product>(query);
            return View(products);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            using var connection = new SqlConnection(_connectionString);
            var query = "SELECT * FROM Products WHERE Id = @Id";
            var product = await connection.QueryFirstOrDefaultAsync<Product>(query, new { Id = id });

            if (product == null) return NotFound();

            return View(product);
        }

        // GET: Products/Create
        [Authorize(Roles = "Admin,Engineering User")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Engineering User")]
        public async Task<IActionResult> Create([Bind("Name,SalePrice,CostPrice,Attachments")] Product product)
        {
            if (ModelState.IsValid)
            {
                product.CurrentVersion = "v1";
                product.Status = ProductStatus.Active;
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }
    }
}
