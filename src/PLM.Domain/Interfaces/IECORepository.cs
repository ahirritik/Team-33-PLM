using PLM.Domain.Entities;
using PLM.Domain.Enums;

namespace PLM.Domain.Interfaces;

public interface IECORepository
{
    Task<ECO?> GetByIdAsync(int id);
    Task<ECO?> GetByIdWithApprovalsAsync(int id);
    Task<IReadOnlyList<ECO>> GetAllAsync(ECOStage? stage = null);
    Task<(IReadOnlyList<ECO> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, ECOStage? stage = null, ECOType? type = null);
    Task<IReadOnlyList<ECO>> GetByProductIdAsync(int productId);
    Task<IReadOnlyList<ECO>> GetPendingApprovalsAsync();
    Task<ECO> AddAsync(ECO eco);
    Task UpdateAsync(ECO eco);
    Task<int> GetCountByStageAsync(ECOStage stage);
}
