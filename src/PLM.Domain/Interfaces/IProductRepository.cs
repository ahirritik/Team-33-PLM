using PLM.Domain.Entities;
using PLM.Domain.Enums;

namespace PLM.Domain.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);
    Task<Product?> GetByIdWithVersionsAsync(int id);
    Task<IReadOnlyList<Product>> GetAllAsync(Status? status = null);
    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search = null, Status? status = null);
    Task<Product> AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task<bool> ExistsAsync(int id);
}
