using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PLM.Data;
using PLM.Models;

namespace PLM.Controllers
{
    [Authorize]
    public class ECOsController : Controller
    {
        private readonly string _connectionString;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ECOsController(IConfiguration configuration, ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
            _context = context;
            _userManager = userManager;
        }

        // GET: ECOs
        public async Task<IActionResult> Index()
        {
            using var connection = new SqlConnection(_connectionString);
            var query = @"
                SELECT e.*, p.Name AS ProductName 
                FROM ECOs e
                LEFT JOIN Products p ON e.ProductId = p.Id
                ORDER BY e.Id DESC";
                
            var ecos = await connection.QueryAsync<ECOViewModel>(query);
            return View(ecos);
        }

        // GET: ECOs/Create
        [Authorize(Roles = "Admin,Engineering User")]
        public async Task<IActionResult> Create(int? productId, int? type)
        {
            using var connection = new SqlConnection(_connectionString);
            var products = await connection.QueryAsync<Product>("SELECT * FROM Products WHERE Status = @Status", new { Status = (int)ProductStatus.Active });
            ViewBag.Products = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(products, "Id", "Name", productId);
            
            var eco = new ECO();
            if (productId.HasValue) eco.ProductId = productId.Value;
            if (type.HasValue) eco.Type = (ECOType)type.Value;
            
            return View(eco);
        }

        // POST: ECOs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Engineering User")]
        public async Task<IActionResult> Create([Bind("Title,Type,ProductId,BoMId,IsVersionUpdate")] ECO eco)
        {
            ModelState.Clear();
            if (string.IsNullOrWhiteSpace(eco.Title)) ModelState.AddModelError("Title", "ECO Title is required.");
            if (eco.ProductId <= 0) ModelState.AddModelError("ProductId", "Target Product is required.");

            // CRITICAL FIX: Coerce BoMId = 0 to null to avoid FK constraint violation
            if (eco.BoMId.HasValue && eco.BoMId.Value == 0) eco.BoMId = null;

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                eco.CreatedByUserId = user?.Id ?? string.Empty;
                eco.EffectiveDate = DateTime.UtcNow;
                eco.Stage = ECOStage.New;
                
                _context.Add(eco);
                await _context.SaveChangesAsync();
                
                // Add first log
                _context.ECOLogs.Add(new ECOLog { ECOId = eco.Id, Action = "Created", UserId = eco.CreatedByUserId, Comments = "ECO Created", Timestamp = DateTime.UtcNow });
                await _context.SaveChangesAsync();
                
                return RedirectToAction(nameof(Details), new { id = eco.Id });
            }
            
            // Repopulate Viewbags if failed
            using var connection = new SqlConnection(_connectionString);
            var products = await connection.QueryAsync<Product>("SELECT * FROM Products WHERE Status = @Status", new { Status = (int)ProductStatus.Active });
            ViewBag.Products = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(products, "Id", "Name", eco.ProductId);
            
            return View(eco);
        }

        // GET: ECOs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var eco = await _context.ECOs.FindAsync(id);
            if (eco == null) return NotFound();

            using var connection = new SqlConnection(_connectionString);
            var query = "SELECT e.*, p.Name AS ProductName FROM ECOs e LEFT JOIN Products p ON e.ProductId = p.Id WHERE e.Id = @Id";
            var vm = await connection.QueryFirstOrDefaultAsync<ECOViewModel>(query, new { Id = id });

            vm.Logs = _context.ECOLogs.Where(l => l.ECOId == id).OrderByDescending(l => l.Timestamp).ToList();

            if (vm.Type == ECOType.Product)
            {
                vm.OldProduct = await _context.Products.FindAsync(vm.ProductId);
            }
            else if (vm.Type == ECOType.BoM && vm.BoMId.HasValue)
            {
                vm.OldBoM = await _context.BoMs.Include(b => b.Components).Include(b => b.Operations).FirstOrDefaultAsync(b => b.Id == vm.BoMId.Value);
            }

            if (!string.IsNullOrEmpty(vm.ProposedChangesJson))
            {
                try
                {
                    if (vm.Type == ECOType.Product)
                    {
                        var rawDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(vm.ProposedChangesJson);
                        if (rawDict != null)
                        {
                            foreach (var kvp in rawDict)
                                vm.ParsedChanges[kvp.Key] = kvp.Value?.ToString() ?? "";
                        }
                    }
                }
                catch { }
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Engineering User")]
        public async Task<IActionResult> SubmitForApproval(int id)
        {
            var eco = await _context.ECOs.FindAsync(id);
            if (eco == null) return NotFound();

            if (eco.Stage == ECOStage.New)
            {
                eco.Stage = ECOStage.Approval;
                
                var user = await _userManager.GetUserAsync(User);
                _context.ECOLogs.Add(new ECOLog { ECOId = eco.Id, Action = "Submitted for Approval", UserId = user?.Id ?? "", Timestamp = DateTime.UtcNow });
                
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Approver")]
        public async Task<IActionResult> Approve(int id)
        {
            var eco = await _context.ECOs.FindAsync(id);
            if (eco == null) return NotFound();

            if (eco.Stage == ECOStage.Approval)
            {
                eco.Stage = ECOStage.Done;
                
                var user = await _userManager.GetUserAsync(User);
                _context.ECOLogs.Add(new ECOLog { ECOId = eco.Id, Action = "Approved and Applied", UserId = user?.Id ?? "", Timestamp = DateTime.UtcNow });
                
                // Apply Diff Logic
                if (!string.IsNullOrEmpty(eco.ProposedChangesJson))
                {
                    try
                    {
                        var changes = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(eco.ProposedChangesJson);
                        if (changes != null || eco.Type == ECOType.BoM)
                        {
                            if (eco.Type == ECOType.Product && changes != null)
                            {
                                var product = await _context.Products.FindAsync(eco.ProductId);
                                if (product != null)
                                {
                                    if (eco.IsVersionUpdate)
                                    {
                                        var newProduct = new Product
                                        {
                                            Name = product.Name,
                                            SalePrice = changes.ContainsKey("SalePrice") ? Convert.ToDecimal(changes["SalePrice"].ToString()) : product.SalePrice,
                                            CostPrice = changes.ContainsKey("CostPrice") ? Convert.ToDecimal(changes["CostPrice"].ToString()) : product.CostPrice,
                                            Attachments = product.Attachments,
                                            CurrentVersion = "v" + (int.Parse(product.CurrentVersion.Replace("v", "")) + 1),
                                            Status = ProductStatus.Active
                                        };
                                        product.Status = ProductStatus.Archived;
                                        _context.Products.Add(newProduct);
                                    }
                                    else
                                    {
                                        if (changes.ContainsKey("SalePrice")) product.SalePrice = Convert.ToDecimal(changes["SalePrice"].ToString());
                                        if (changes.ContainsKey("CostPrice")) product.CostPrice = Convert.ToDecimal(changes["CostPrice"].ToString());
                                        if (changes.ContainsKey("Name")) product.Name = changes["Name"].ToString() ?? product.Name;
                                    }
                                }
                            }
                            else if (eco.Type == ECOType.BoM && eco.BoMId.HasValue)
                            {
                                var oldBoM = await _context.BoMs.Include(b => b.Components).Include(b => b.Operations).FirstOrDefaultAsync(b => b.Id == eco.BoMId.Value);
                                if (oldBoM != null)
                                {
                                    var bomChanges = System.Text.Json.JsonSerializer.Deserialize<BoMDiffDTO>(eco.ProposedChangesJson, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                                    if (bomChanges != null)
                                    {
                                        if (eco.IsVersionUpdate)
                                        {
                                            var newBom = new BoM {
                                                ProductId = oldBoM.ProductId,
                                                Version = "v" + (int.Parse(oldBoM.Version.Replace("v", "")) + 1),
                                                Status = BoMStatus.Active,
                                                Components = bomChanges.Components.Select(c => new BoMComponent { ProductId = c.ProductId, Quantity = c.Quantity }).ToList(),
                                                Operations = bomChanges.Operations.Select(o => new BoMOperation { Name = o.Name, TimeMinutes = o.TimeMinutes, WorkCenter = o.WorkCenter }).ToList()
                                            };
                                            oldBoM.Status = BoMStatus.Archived;
                                            _context.BoMs.Add(newBom);
                                        }
                                        else
                                        {
                                            _context.BoMComponents.RemoveRange(oldBoM.Components);
                                            _context.BoMOperations.RemoveRange(oldBoM.Operations);
                                            oldBoM.Components = bomChanges.Components.Select(c => new BoMComponent { ProductId = c.ProductId, Quantity = c.Quantity }).ToList();
                                            oldBoM.Operations = bomChanges.Operations.Select(o => new BoMOperation { Name = o.Name, TimeMinutes = o.TimeMinutes, WorkCenter = o.WorkCenter }).ToList();
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _context.ECOLogs.Add(new ECOLog { ECOId = eco.Id, Action = "Apply Failed", UserId = "System", Comments = ex.Message, Timestamp = DateTime.UtcNow });
                    }
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Approver")]
        public async Task<IActionResult> Reject(int id)
        {
            var eco = await _context.ECOs.FindAsync(id);
            if (eco == null) return NotFound();

            if (eco.Stage == ECOStage.Approval)
            {
                eco.Stage = ECOStage.New;
                
                var user = await _userManager.GetUserAsync(User);
                _context.ECOLogs.Add(new ECOLog { ECOId = eco.Id, Action = "Rejected to Draft", UserId = user?.Id ?? "", Timestamp = DateTime.UtcNow });
                
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = id });
        }

        // GET: ECOs/EditDraft/5
        [Authorize(Roles = "Admin,Engineering User")]
        public async Task<IActionResult> EditDraft(int? id)
        {
            if (id == null) return NotFound();

            var eco = await _context.ECOs.FindAsync(id);
            if (eco == null) return NotFound();

            if (eco.Stage != ECOStage.New)
                return RedirectToAction(nameof(Details), new { id = id }); // Can only edit in draft

            return View(eco);
        }

        // POST: ECOs/EditDraft/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Engineering User")]
        public async Task<IActionResult> EditDraft(int id, [Bind("Id,ProposedChangesJson")] ECO ecoModel)
        {
            if (id != ecoModel.Id) return NotFound();

            var eco = await _context.ECOs.FindAsync(id);
            if (eco == null) return NotFound();

            if (eco.Stage == ECOStage.New)
            {
                eco.ProposedChangesJson = ecoModel.ProposedChangesJson;
                
                var user = await _userManager.GetUserAsync(User);
                _context.ECOLogs.Add(new ECOLog { ECOId = eco.Id, Action = "Draft Updated", UserId = user?.Id ?? "", Timestamp = DateTime.UtcNow });
                
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = eco.Id });
        }
    }

    public class ECOViewModel
    {
        public int Id { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public ECOType Type { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int? BoMId { get; set; }
        public string CreatedByUserId { get; set; } = string.Empty;
        public DateTime EffectiveDate { get; set; }
        public bool IsVersionUpdate { get; set; }
        public ECOStage Stage { get; set; }
        public string ProposedChangesJson { get; set; } = string.Empty;
        public List<ECOLog> Logs { get; set; } = new();
        public Product? OldProduct { get; set; }
        public BoM? OldBoM { get; set; }
        public Dictionary<string, string> ParsedChanges { get; set; } = new();
    }

    public class BoMDiffDTO
    {
        public List<BoMComponentDiff> Components { get; set; } = new();
        public List<BoMOperationDiff> Operations { get; set; } = new();
    }

    public class BoMComponentDiff
    {
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
    }

    public class BoMOperationDiff
    {
        public string Name { get; set; } = string.Empty;
        public int TimeMinutes { get; set; }
        public string WorkCenter { get; set; } = string.Empty;
    }
}
