using Microsoft.AspNetCore.Mvc;
using LMS_SoulCode.Features.Security.Services;
using LMS_SoulCode.Features.Common;
using Microsoft.AspNetCore.Authorization;
using StatusCodes = LMS_SoulCode.Features.Common.StatusCodes;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace LMS_SoulCode.Features.Security.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CryptoController : BaseApiController
    {
        private readonly CryptographyService _crypto;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        public CryptoController(CryptographyService crypto, IConfiguration config, ILogger<CryptoController> logger, IWebHostEnvironment env) : base(logger)
        {
            _crypto = crypto;
            _config = config;
            _env = env;
        }

        [HttpPost("upload-file")]
        [BackOfficePermission(ModuleCodes.FILE_MANAGEMENT, PermissionCodes.FILE_UPLOAD)]
        public async Task<IActionResult> UploadFile(IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
            {
                var error = ApiResponse<List<string>>.Fail("No file selected.", StatusCodes.BadRequest);
                return StatusCode(error.Code, error);
            }

            try
            {
                string ext = Path.GetExtension(file.FileName).ToLower();
                string folderPath = GetPathByExt(ext);

                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                using var ms = new MemoryStream();
                await file.CopyToAsync(ms, cancellationToken);
                
                string savePath = Path.Combine(folderPath, file.FileName + ".enc");

                if (IsImage(ext))
                {
                    using var processedMs = new MemoryStream(await _crypto.ProcessImageAsync(ms, 1200, cancellationToken));
                    await _crypto.EncryptLargeFileAsync(processedMs, savePath, cancellationToken);
                }
                else
                {
                    ms.Position = 0;
                    await _crypto.EncryptLargeFileAsync(ms, savePath, cancellationToken);
                }

                var response = ApiResponse<List<object>>.Success(new List<object> { new { path = savePath.Replace("\\", "/") } }, Messages.Uploaded);
                return StatusCode(response.Code, response);
            }
            catch (Exception ex)
            {
                var error = ApiResponse<List<string>>.Fail(ex.Message, StatusCodes.ServerError);
                return StatusCode(error.Code, error);
            }
        }

        [HttpPost("upload-secure-with-thumb")]
        [BackOfficePermission(ModuleCodes.FILE_MANAGEMENT, PermissionCodes.FILE_UPLOAD)]
        public async Task<IActionResult> UploadSecureWithThumb(IFormFile file, CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
            {
                var error = ApiResponse<List<string>>.Fail("No file selected.", StatusCodes.BadRequest);
                return StatusCode(error.Code, error);
            }

            try
            {
                string ext = Path.GetExtension(file.FileName).ToLower();
                if (!IsImage(ext)) 
                {
                    var error = ApiResponse<List<string>>.Fail("Only image files are supported.", StatusCodes.BadRequest);
                    return StatusCode(error.Code, error);
                }

                string folderPath = _config["AppSettings:OrgLogosPath"] ?? "wwwroot/uploads/OrgLogos";
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                using var ms = new MemoryStream();
                await file.CopyToAsync(ms, cancellationToken);

                var thumbSize = int.Parse(_config["AppSettings:OrgLogoThumbSize"] ?? "150");
                var (mainBytes, thumbBytes) = await _crypto.ProcessImageWithThumbAsync(ms, thumbSize, cancellationToken);

                string fileGuid = Guid.NewGuid().ToString();
                string mainFileName = $"{fileGuid}_main{ext}.enc";
                string thumbFileName = $"{fileGuid}_thumb{ext}.enc";

                string mainPath = Path.Combine(folderPath, mainFileName);
                string thumbPath = Path.Combine(folderPath, thumbFileName);

                await _crypto.EncryptLargeFileAsync(new MemoryStream(mainBytes), mainPath, cancellationToken);
                await _crypto.EncryptLargeFileAsync(new MemoryStream(thumbBytes), thumbPath, cancellationToken);

                var response = ApiResponse<List<object>>.Success(new List<object> { new 
                { 
                    mainPath = mainPath.Replace("\\", "/"), 
                    thumbPath = thumbPath.Replace("\\", "/") 
                } }, Messages.Uploaded);

                return StatusCode(response.Code, response);
            }
            catch (Exception ex)
            {
                var error = ApiResponse<List<string>>.Fail(ex.Message, StatusCodes.ServerError);
                return StatusCode(error.Code, error);
            }
        }

        [HttpGet("get")]
        [AllowAnonymous]
        public IActionResult GetMedia([FromQuery] string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path)) return BadRequest("Path is required.");

                var normalizedPath = path.Replace("/", "\\").TrimStart('\\');
                // Use ContentRootPath instead of GetCurrentDirectory for reliability
                var fullPath = Path.Combine(_env.ContentRootPath, normalizedPath);

                if (!System.IO.File.Exists(fullPath)) 
                {
                    // Fallback: Check without 'wwwroot' prefix if path already includes it and file not found
                    // Or if CurrentDirectory works better in some environments
                    var altPath = Path.Combine(Directory.GetCurrentDirectory(), normalizedPath);
                    if (System.IO.File.Exists(altPath))
                    {
                        fullPath = altPath;
                    }
                    else if (normalizedPath.StartsWith("wwwroot\\"))
                    {
                         var noWwwRoot = normalizedPath.Substring(8);
                         var webRootPath = Path.Combine(_env.ContentRootPath, "wwwroot", noWwwRoot);
                         if (System.IO.File.Exists(webRootPath)) fullPath = webRootPath;
                         else return NotFound($"File not found. Path: {normalizedPath}");
                    }
                    else
                    {
                        return NotFound($"File not found. Path: {normalizedPath}");
                    }
                }

                string cleanPath = path.Replace(".enc", "");
                string ext = Path.GetExtension(cleanPath).ToLower();
                
                var stream = _crypto.GetDecryptStream(fullPath);

                if (ext == ".mp4" || ext == ".webm" || ext == ".mkv")
                {
                    return new FileStreamResult(stream, "video/mp4") { EnableRangeProcessing = true };
                }

                return File(stream, GetContentType(ext));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private string GetPathByExt(string ext) => ext switch
        {
            ".mp4" or ".avi" or ".mkv" => _config["AppSettings:VideosPath"] ?? "wwwroot/uploads/videos",
            ".pdf" or ".doc" or ".docx" => _config["AppSettings:DocsPath"] ?? "wwwroot/uploads/documents",
            _ => _config["AppSettings:OrgLogosPath"] ?? "wwwroot/uploads/OrgLogos"
        };

        private bool IsImage(string ext) => ext switch
        {
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" => true,
            _ => false
        };

        private string GetContentType(string ext) => ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".pdf" => "application/pdf",
            ".mp4" => "video/mp4",
            _ => "application/octet-stream"
        };
    }
}
