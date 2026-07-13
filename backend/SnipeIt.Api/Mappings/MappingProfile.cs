using AutoMapper;
using SnipeIt.Api.Dtos;
using SnipeIt.Api.Models.Entities;

namespace SnipeIt.Api.Mappings;

/// <summary>
/// AutoMapper profile for mapping between entities and DTOs
/// </summary>
public sealed class MappingProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the MappingProfile class
    /// </summary>
    public MappingProfile()
    {
        // Asset mappings
        CreateMap<Asset, AssetResponseDto>()
            .ReverseMap();

        CreateMap<CreateAssetDto, Asset>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());

        CreateMap<UpdateAssetDto, Asset>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // User mappings (entity -> response only; writes are handled explicitly in the service
        // so password/security fields never round-trip through AutoMapper).
        CreateMap<User, UserResponseDto>();
    }
}
