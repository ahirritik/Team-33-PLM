using AutoMapper;
using PLM.Application.DTOs;
using PLM.Application.Interfaces;
using PLM.Domain.Entities;
using PLM.Domain.Enums;
using PLM.Domain.Interfaces;

namespace PLM.Application.Services;

public class BoMService : IBoMService
{
    private readonly IBoMRepository _bomRepo;
    private readonly IBoMVersionRepository _versionRepo;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public BoMService(
        IBoMRepository bomRepo,
        IBoMVersionRepository versionRepo,
        IAuditService auditService,
        IMapper mapper)
    {
        _bomRepo = bomRepo;
        _versionRepo = versionRepo;
        _auditService = auditService;
        _mapper = mapper;
    }

    public async Task<BoMDto?> GetByIdAsync(int id)
    {
        var bom = await _bomRepo.GetByIdWithVersionsAsync(id);
        return bom == null ? null : _mapper.Map<BoMDto>(bom);
    }

    public async Task<BoMDto?> GetByIdWithVersionsAsync(int id)
    {
        var bom = await _bomRepo.GetByIdWithVersionsAsync(id);
        return bom == null ? null : _mapper.Map<BoMDto>(bom);
    }

    public async Task<(List<BoMDto> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search = null)
    {
        var (items, totalCount) = await _bomRepo.GetPagedAsync(page, pageSize, search);
        return (_mapper.Map<List<BoMDto>>(items), totalCount);
    }

    public async Task<List<BoMDto>> GetByProductIdAsync(int productId)
    {
        var boms = await _bomRepo.GetByProductIdAsync(productId);
        return _mapper.Map<List<BoMDto>>(boms);
    }

    public async Task<BoMDto> CreateAsync(BoMCreateDto dto, string userId, string userName)
    {
        var bom = new BoM
        {
            ProductId = dto.ProductId,
            Name = dto.Name,
            Description = dto.Description,
            Status = Status.Active,
            CurrentVersionNumber = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _bomRepo.AddAsync(bom);

        // Create initial version with components and operations
        var version = new BoMVersion
        {
            BoMId = created.Id,
            VersionNumber = 1,
            IsActive = true,
            ChangeDescription = "Initial version",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userName,
            Components = dto.Components.Select(c => new BoMComponent
            {
                ComponentName = c.ComponentName,
                PartNumber = c.PartNumber,
                Quantity = c.Quantity,
                UnitCost = c.UnitCost,
                Unit = c.Unit
            }).ToList(),
            Operations = dto.Operations.Select(o => new BoMOperation
            {
                OperationName = o.OperationName,
                Description = o.Description,
                SequenceOrder = o.SequenceOrder,
                EstimatedTime = o.EstimatedTime,
                WorkCenter = o.WorkCenter
            }).ToList()
        };

        await _versionRepo.AddAsync(version);

        await _auditService.LogAsync(
            "BoM Created",
            "BoM",
            created.Id,
            null,
            $"Name: {created.Name}, Components: {dto.Components.Count}",
            userId,
            userName);

        return _mapper.Map<BoMDto>(await _bomRepo.GetByIdWithVersionsAsync(created.Id));
    }

    public async Task<BoMVersionDto?> GetVersionWithComponentsAsync(int versionId)
    {
        var version = await _versionRepo.GetByIdWithComponentsAsync(versionId);
        return version == null ? null : _mapper.Map<BoMVersionDto>(version);
    }

    public async Task<List<BoMVersionDto>> GetVersionHistoryAsync(int bomId)
    {
        var versions = await _versionRepo.GetByBoMIdAsync(bomId);
        return _mapper.Map<List<BoMVersionDto>>(versions);
    }
}
