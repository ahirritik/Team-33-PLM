using PLM.Domain.Entities;
using PLM.Domain.Enums;

namespace PLM.Domain.Interfaces;

public interface IBoMRepository
{
    Task<BoM?> GetByIdAsync(int id);
    Task<BoM?> GetByIdWithVersionsAsync(int id);
    Task<IReadOnlyList<BoM>> GetByProductIdAsync(int productId);
    Task<(IReadOnlyList<BoM> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search = null, Status? status = null);
    Task<BoM> AddAsync(BoM bom);
    Task UpdateAsync(BoM bom);
    Task<bool> ExistsAsync(int id);
}
