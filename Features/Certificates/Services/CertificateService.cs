using AutoMapper;
using LMS_SoulCode.Features.Certificates.DTOs;
using LMS_SoulCode.Features.Certificates.Models;
using LMS_SoulCode.Features.Certificates.Repositories;
using LMS_SoulCode.Features.Common;
using StatusCodes = LMS_SoulCode.Features.Common.StatusCodes;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using BCrypt.Net;
using LMS_SoulCode.Features.Security.Services;

namespace LMS_SoulCode.Features.Certificates.Services
{
    public interface ICertificateService
    {
        Task<ApiResponse<List<CertificateDto>>> GenerateCertificateAsync(CreateCertificateRequest request, int? tenantId, CancellationToken cancellationToken = default);
        Task<ApiResponse<List<CertificateDto>>> ValidateByCodeAsync(string code, int? tenantId, CancellationToken cancellationToken = default);
        Task<FileResult?> GetPdfAsync(int id, int? tenantId, CancellationToken cancellationToken = default); // for download
        Task<ApiResponse<IEnumerable<CertificateDto>>> GetCertificatesByUserAsync(int userId, int? tenantId, CancellationToken cancellationToken = default);
        Task<ApiResponse<IEnumerable<CertificateDto>>> GetAllCertificatesAsync(int? tenantId, CancellationToken cancellationToken = default);
        Task<PagedApiResponse<CertificateListDto>> GetCertificatesAsync(CertificateListRequest request, int? tenantId, CancellationToken cancellationToken);
        Task<ApiResponse<List<string>>> RevokeCertificateAsync(int id, int? tenantId, CancellationToken cancellationToken = default);
    }
    public class CertificateService : ICertificateService
    {
        private readonly ICertificateRepository _certificateRepo;
        private readonly IWebHostEnvironment _env; // for saving local files
        private readonly CryptographyService _cryptoService;
        private readonly IMapper _mapper;

        public CertificateService(ICertificateRepository certificateRepository, IWebHostEnvironment env, CryptographyService cryptographyService, IMapper mapper)
        {
            _certificateRepo = certificateRepository;
            _env = env;
            _cryptoService = cryptographyService;
            _mapper = mapper;
        }

        public async Task<ApiResponse<List<CertificateDto>>> GenerateCertificateAsync(CreateCertificateRequest request, int? tenantId, CancellationToken cancellationToken = default)
        {
            // 1) (Optional) Validate completion logic: check if user completed course
            // assume validated or caller verified before calling

            // 2) create metadata
            var code = GenerateShortCode();
            var cert = _mapper.Map<Certificate>(request);
            cert.CertificateCode = code;
            cert.TenantId = tenantId;

            // 3) generate pdf bytes
            var pdfBytes = GeneratePdfBytes(cert);
            string encryptedBase64 = _cryptoService.EncryptBytes(pdfBytes);

            // 4) save file (local example)
            var uploads = Path.Combine(_env.WebRootPath ?? "wwwroot", "certificates");
            if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);
            var fileName = $"certificate_{cert.CertificateCode}.pdf.enc";
            var fullPath = Path.Combine(uploads, fileName);
            
            // Fix: Write the ENCRYPTED BASE64 string, not the plain bytes
            await File.WriteAllTextAsync(fullPath, encryptedBase64, cancellationToken);

            cert.FilePath = $"/certificates/{fileName}"; // relative url; use blob url if stored in cloud

            await _certificateRepo.AddAsync(cert, cancellationToken);

            var response = _mapper.Map<CertificateDto>(cert);
            return ApiResponse<List<CertificateDto>>.Success(new List<CertificateDto> { response }, Messages.Created);
        }

        public async Task<ApiResponse<List<CertificateDto>>> ValidateByCodeAsync(string code, int? tenantId, CancellationToken cancellationToken = default)
        {
            var cert = await _certificateRepo.GetByCodeAsync(code, tenantId, cancellationToken);
            if (cert == null || cert.IsRevoked) 
                return ApiResponse<List<CertificateDto>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            var dto = _mapper.Map<CertificateDto>(cert);
            return ApiResponse<List<CertificateDto>>.Success(new List<CertificateDto> { dto }, Messages.Success);
        }

        public async Task<FileResult?> GetPdfAsync(int id, int? tenantId, CancellationToken cancellationToken = default)
        {
            var cert = await _certificateRepo.GetByIdAsync(id, tenantId, cancellationToken);
            if (cert == null) return null;

            var path = Path.Combine(_env.WebRootPath ?? "wwwroot", cert.FilePath.TrimStart('/'));
            if (!System.IO.File.Exists(path)) return null;

            // Fix: Read as string (Base64)
            string encryptedBase64 = await System.IO.File.ReadAllTextAsync(path, cancellationToken);
            // No need to Convert.ToBase64String because it IS a Base64 string provided by EncryptBytes

            byte[] plainBytes = _cryptoService.DecryptBytes(encryptedBase64);

            return new FileContentResult(plainBytes, "application/pdf")
            {
                FileDownloadName = Path.GetFileName(path).Replace(".enc", "")
            };
        }

        // New paginated method
        public async Task<PagedApiResponse<CertificateListDto>> GetCertificatesAsync(CertificateListRequest request, int? tenantId, CancellationToken cancellationToken)
        {
            // Security: Org Admin can only see their own tenant, SuperAdmin can specify or see all
            var targetTenantId = tenantId.HasValue 
                ? tenantId                  // Org Admin - force their own tenant
                : request.TenantId;         // SuperAdmin - use request or null
            
            var (certificates, totalCount) = await _certificateRepo.GetCertificatesAsync(
                request.SearchTerm, 
                request.PageNumber, 
                request.PageSize, 
                request.UserId, 
                request.CourseId, 
                request.IssuedFrom, 
                request.IssuedTo, 
                targetTenantId,
                cancellationToken);

            return PagedApiResponse<CertificateListDto>.Success(certificates, request.PageNumber, request.PageSize, totalCount, Messages.Success);
        }

        // helpers
        private string GenerateShortCode()
        {
            // generate short friendly code (8 chars)
            return Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
        }
        public async Task<ApiResponse<IEnumerable<CertificateDto>>> GetCertificatesByUserAsync(int userId, int? tenantId, CancellationToken cancellationToken = default)
        {
            var list = await _certificateRepo.GetByUserIdAsync(userId, tenantId, cancellationToken);
            var response = _mapper.Map<IEnumerable<CertificateDto>>(list);
            return ApiResponse<IEnumerable<CertificateDto>>.Success(response, Messages.Success);
        }

        public async Task<ApiResponse<IEnumerable<CertificateDto>>> GetAllCertificatesAsync(int? tenantId, CancellationToken cancellationToken = default)
        {
            var list = await _certificateRepo.GetAllAsync(tenantId, cancellationToken);
            var response = _mapper.Map<IEnumerable<CertificateDto>>(list);
            return ApiResponse<IEnumerable<CertificateDto>>.Success(response, Messages.Success);
        }

        public async Task<ApiResponse<List<string>>> RevokeCertificateAsync(int id, int? tenantId, CancellationToken cancellationToken = default)
        {
            var cert = await _certificateRepo.GetByIdAsync(id, tenantId, cancellationToken);
            
            if (cert == null) 
                return ApiResponse<List<string>>.Fail(Messages.NotFound, StatusCodes.NotFound);

            cert.IsRevoked = true;
            await _certificateRepo.UpdateAsync(cert, cancellationToken);
            return ApiResponse<List<string>>.Success(new List<string>(), "Certificate revoked successfully.");
        }

        private byte[] GeneratePdfBytes(Certificate cert)
        {
            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(50);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(14));

                    page.Header()
                        .Text("Certificate of Completion")
                        .FontSize(24)
                        .Bold()
                        .AlignCenter();

                    page.Content().Column(col =>
                    {
                        col.Item().Text($"This is to certify that").AlignCenter();
                        col.Item().Text("[Student Name]").FontSize(20).Bold().AlignCenter();
                        col.Item().Text("has successfully completed the course").AlignCenter();
                        col.Item().Text("[Course Title]").FontSize(18).SemiBold().AlignCenter();
                        col.Item().Text($"Score: {cert.Score ?? 0}").AlignCenter();
                        col.Item().Text($"Issued On: {cert.IssuedAt:yyyy-MM-dd}").AlignCenter();
                        col.Item().Text($"Certificate Code: {cert.CertificateCode}").AlignCenter();
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text("Verification URL: /verify/" + cert.CertificateCode)
                        .FontSize(10);
                });
            });

            return document.GeneratePdf();
        }

    }

}
