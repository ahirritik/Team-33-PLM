using PLM.Domain.Entities;

namespace PLM.Domain.Interfaces;

public interface IBoMVersionRepository
{
    Task<BoMVersion?> GetByIdAsync(int id);
    Task<BoMVersion?> GetByIdWithComponentsAsync(int id);
    Task<IReadOnlyList<BoMVersion>> GetByBoMIdAsync(int bomId);
    Task<BoMVersion?> GetActiveVersionAsync(int bomId);
    Task<BoMVersion> AddAsync(BoMVersion version);
    Task UpdateAsync(BoMVersion version);
}
