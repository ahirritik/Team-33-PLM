using AutoMapper;
using PLM.Application.DTOs;
using PLM.Domain.Entities;

namespace PLM.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Product
        CreateMap<Product, ProductDto>()
            .ForMember(d => d.VersionCount, opt => opt.MapFrom(s => s.Versions.Count))
            .ForMember(d => d.ActiveVersion, opt => opt.MapFrom(s => s.Versions.FirstOrDefault(v => v.IsActive)));
        CreateMap<ProductVersion, ProductVersionDto>();
        CreateMap<ProductCreateDto, Product>();

        // BoM
        CreateMap<BoM, BoMDto>()
            .ForMember(d => d.ProductName, opt => opt.MapFrom(s => s.Product.Name))
            .ForMember(d => d.VersionCount, opt => opt.MapFrom(s => s.Versions.Count))
            .ForMember(d => d.ActiveVersion, opt => opt.MapFrom(s => s.Versions.FirstOrDefault(v => v.IsActive)));
        CreateMap<BoMVersion, BoMVersionDto>();
        CreateMap<BoMComponent, BoMComponentDto>().ReverseMap();
        CreateMap<BoMOperation, BoMOperationDto>().ReverseMap();

        // ECO
        CreateMap<ECO, ECODto>()
            .ForMember(d => d.ProductName, opt => opt.MapFrom(s => s.Product.Name))
            .ForMember(d => d.BoMName, opt => opt.MapFrom(s => s.BoM != null ? s.BoM.Name : null));
        CreateMap<ECOCreateDto, ECO>();
        CreateMap<ECOApproval, ECOApprovalDto>();

        // AuditLog
        CreateMap<AuditLog, AuditLogDto>();
    }
}
