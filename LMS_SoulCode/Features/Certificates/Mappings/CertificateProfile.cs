using AutoMapper;
using LMS_SoulCode.Features.Certificates.DTOs;
using LMS_SoulCode.Features.Certificates.Models;

namespace LMS_SoulCode.Features.Certificates.Mappings
{
    public class CertificateProfile : Profile
    {
        public CertificateProfile()
        {
            // Certificate to CertificateDto mapping
            CreateMap<Certificate, CertificateDto>()
                .ForMember(d => d.FileUrl, o => o.MapFrom(s => s.FilePath));

            // Certificate to CertificateListDto mapping (for paginated lists)
            CreateMap<Certificate, CertificateListDto>()
                .ForMember(d => d.FileUrl, o => o.MapFrom(s => s.FilePath))
                .ForMember(d => d.UserName, o => o.Ignore()) // Will be populated from joined data
                .ForMember(d => d.UserEmail, o => o.Ignore()) // Will be populated from joined data
                .ForMember(d => d.CourseTitle, o => o.Ignore()); // Will be populated from joined data

            // CreateCertificateRequest to Certificate mapping
            CreateMap<CreateCertificateRequest, Certificate>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.CertificateCode, o => o.Ignore()) // Will be generated
                .ForMember(d => d.IssuedAt, o => o.MapFrom(_ => DateTime.UtcNow))
                .ForMember(d => d.FilePath, o => o.Ignore()) // Will be set after PDF generation
                .ForMember(d => d.IsRevoked, o => o.MapFrom(_ => false));
            
            CreateMap<CertificateTemplate, CertificateTemplateDto>();
        }
    }
}