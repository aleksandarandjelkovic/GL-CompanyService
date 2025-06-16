using AutoMapper;
using Company.Application.DTOs;

namespace Company.Application.Mapping;

/// <summary>
/// AutoMapper profile for mapping between company entities and DTOs.
/// </summary>
public class CompanyMappingProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CompanyMappingProfile"/> class.
    /// </summary>
    public CompanyMappingProfile()
    {
        // Entity to Response mappings
        CreateMap<Domain.Entities.Company, CompanyResponse>();
    }
}