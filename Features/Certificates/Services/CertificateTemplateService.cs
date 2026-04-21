using AutoMapper;
using LMS_SoulCode.Features.Certificates.DTOs;
using LMS_SoulCode.Features.Common;
using StatusCodes = LMS_SoulCode.Features.Common.StatusCodes;
using LMS_SoulCode.Features.Certificates.Models;
using LMS_SoulCode.Features.Certificates.Repositories;
using LMS_SoulCode.Features.Security.Services;

namespace LMS_SoulCode.Features.Certificates.Services
{
    public class CertificateTemplateService : ICertificateTemplateService
    {
        private readonly ICertificateTemplateRepository _repository;
        private readonly IWebHostEnvironment _env;
        private readonly IMapper _mapper;
        private readonly CryptographyService _crypto;

        public CertificateTemplateService(ICertificateTemplateRepository repository, IWebHostEnvironment env, CryptographyService crypto, IMapper mapper)
        {
            _repository = repository;
            _env = env;
            _crypto = crypto;
            _mapper = mapper;
        }

        public async Task<ApiResponse<CertificateTemplateDto>> CreateTemplateAsync(CreateCertificateTemplateRequest request, CancellationToken cancellationToken = default)
        {
            // 1. Prepare Paths
            var templatesFolder = Path.Combine(_env.WebRootPath, "templates", request.TenantId.ToString());
            if (!Directory.Exists(templatesFolder)) Directory.CreateDirectory(templatesFolder);

            var uniqueId = Guid.NewGuid().ToString("N");
            // Expecting .cshtml file
            var extension = Path.GetExtension(request.TemplateFile.FileName);
            if(string.IsNullOrEmpty(extension)) extension = ".cshtml";

            var encryptedFileName = $"{uniqueId}_template{extension}.enc"; 
            var fullPath = Path.Combine(templatesFolder, encryptedFileName);

            // 2. Read File Bytes
            byte[] fileBytes;
            using (var memoryStream = new MemoryStream())
            {
                await request.TemplateFile.CopyToAsync(memoryStream, cancellationToken);
                fileBytes = memoryStream.ToArray();
            }

            // 3. Encrypt Bytes
            var encryptedBase64 = _crypto.EncryptBytes(fileBytes);
            
            // 4. Save Encrypted File
            await File.WriteAllTextAsync(fullPath, encryptedBase64, cancellationToken);

            // 5. Create Entity
            var template = new CertificateTemplate
            {
                TenantId = request.TenantId,
                Name = request.Name,
                TemplateFilePath = $"/templates/{request.TenantId}/{encryptedFileName}",
                MetadataConfig = request.MetadataConfig,
                IsActive = true
            };

            await _repository.AddAsync(template, cancellationToken);
            
            var response = _mapper.Map<CertificateTemplateDto>(template);
            return ApiResponse<CertificateTemplateDto>.Success(response, Messages.Created);
        }

        public async Task<ApiResponse<IEnumerable<CertificateTemplateDto>>> GetTemplatesByTenantIdAsync(int tenantId, CancellationToken cancellationToken = default)
        {
            var templates = await _repository.GetByTenantIdAsync(tenantId, cancellationToken);
            var response = _mapper.Map<IEnumerable<CertificateTemplateDto>>(templates);
            return ApiResponse<IEnumerable<CertificateTemplateDto>>.Success(response, Messages.Success);
        }

        public async Task<ApiResponse<CertificateTemplateDto>> GetActiveTemplateAsync(int tenantId, CancellationToken cancellationToken = default)
        {
            var template = await _repository.GetActiveTemplateAsync(tenantId, cancellationToken);
            if (template == null)
                return ApiResponse<CertificateTemplateDto>.Fail(Messages.NotFound, StatusCodes.NotFound);

            var response = _mapper.Map<CertificateTemplateDto>(template);
            return ApiResponse<CertificateTemplateDto>.Success(response, Messages.Success);
        }

        public async Task<ApiResponse<string>> DeleteTemplateAsync(int templateId, CancellationToken cancellationToken = default)
        {
            var template = await _repository.GetByIdAsync(templateId, cancellationToken);
            if (template == null)
                return ApiResponse<string>.Fail(Messages.NotFound, StatusCodes.NotFound);

            await _repository.DeleteAsync(templateId, cancellationToken);
            return ApiResponse<string>.Success(null, Messages.Deleted);
        }
    }
}
