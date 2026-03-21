using PLM.Domain.Entities;

namespace PLM.Domain.Interfaces;

public interface IProductVersionRepository
{
    Task<ProductVersion?> GetByIdAsync(int id);
    Task<IReadOnlyList<ProductVersion>> GetByProductIdAsync(int productId);
    Task<ProductVersion?> GetActiveVersionAsync(int productId);
    Task<ProductVersion> AddAsync(ProductVersion version);
    Task UpdateAsync(ProductVersion version);
}
