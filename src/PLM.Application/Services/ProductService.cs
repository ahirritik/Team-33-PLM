using AutoMapper;
using PLM.Application.DTOs;
using PLM.Application.Interfaces;
using PLM.Domain.Entities;
using PLM.Domain.Enums;
using PLM.Domain.Interfaces;

namespace PLM.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepo;
    private readonly IProductVersionRepository _versionRepo;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public ProductService(
        IProductRepository productRepo,
        IProductVersionRepository versionRepo,
        IAuditService auditService,
        IMapper mapper)
    {
        _productRepo = productRepo;
        _versionRepo = versionRepo;
        _auditService = auditService;
        _mapper = mapper;
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _productRepo.GetByIdWithVersionsAsync(id);
        return product == null ? null : _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto?> GetByIdWithVersionsAsync(int id)
    {
        var product = await _productRepo.GetByIdWithVersionsAsync(id);
        return product == null ? null : _mapper.Map<ProductDto>(product);
    }

    public async Task<(List<ProductDto> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search = null)
    {
        var (items, totalCount) = await _productRepo.GetPagedAsync(page, pageSize, search);
        return (_mapper.Map<List<ProductDto>>(items), totalCount);
    }

    public async Task<ProductDto> CreateAsync(ProductCreateDto dto, string userId, string userName)
    {
        var product = _mapper.Map<Product>(dto);
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;
        product.CurrentVersionNumber = 1;
        product.Status = Status.Active;

        var created = await _productRepo.AddAsync(product);

        // Create the initial version
        var version = new ProductVersion
        {
            ProductId = created.Id,
            VersionNumber = 1,
            CostPrice = dto.CostPrice,
            SalePrice = dto.SalePrice,
            IsActive = true,
            ChangeDescription = "Initial version",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userName
        };

        await _versionRepo.AddAsync(version);

        await _auditService.LogAsync(
            "Product Created",
            "Product",
            created.Id,
            null,
            $"Name: {created.Name}, Cost: {dto.CostPrice}, Sale: {dto.SalePrice}",
            userId,
            userName);

        return _mapper.Map<ProductDto>(await _productRepo.GetByIdWithVersionsAsync(created.Id));
    }

    public async Task<List<ProductVersionDto>> GetVersionHistoryAsync(int productId)
    {
        var versions = await _versionRepo.GetByProductIdAsync(productId);
        return _mapper.Map<List<ProductVersionDto>>(versions);
    }
}
