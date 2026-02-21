using Microsoft.AspNetCore.Mvc;
using LMS_SoulCode.Features.Security.Services;
using LMS_SoulCode.Features.Common;
using Microsoft.AspNetCore.Authorization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using StatusCodes = LMS_SoulCode.Features.Common.StatusCodes;

namespace LMS_SoulCode.Features.Security.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CryptoController : BaseApiController
    {
        private readonly CryptographyService _crypto;

        public CryptoController(CryptographyService crypto, ILogger<CryptoController> logger) : base(logger)
        {
            _crypto = crypto;
        }

        //[HttpPost("encrypt")]
        //public IActionResult Encrypt([FromBody] object data)
        //{
        //    string encrypted = _crypto.EncryptDynamic(data);
        //    return Ok(encrypted);
        //}

        //[HttpPost("decrypt")]
        //public IActionResult Decrypt([FromBody] string encrypted)
        //{
        //    // Dynamic object me convert karega
        //    var obj = _crypto.Decrypt(encrypted);
        //    return Ok(obj);
        //}

        [HttpPost("upload-file")]
        [BackOfficePermission(ModuleCodes.FILE_MANAGEMENT, PermissionCodes.FILE_UPLOAD)]
        public async Task<IActionResult> UploadFile(IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
            {
                var errorResponse = ApiResponse<List<string>>.Fail("No file selected.", StatusCodes.BadRequest);
                return StatusCode(errorResponse.Code, errorResponse);
            }

            // 🔹 1. Read file into byte array
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms, cancellationToken);
            var fileBytes = ms.ToArray();
            // 🔹 2. Encrypt file content
            string encryptedData = _crypto.EncryptBytes(fileBytes);
            // 🔹 3. Identify folder based on file extension
            string ext = Path.GetExtension(file.FileName).ToLower();
            string folderName = ext switch
            {
                ".jpg" or ".jpeg" or ".png" or ".gif" => "images",
                ".mp4" or ".avi" or ".mov" => "videos",
                ".pdf" => "pdfs",
                ".doc" or ".docx" => "documents",
                ".xls" or ".xlsx" => "excels",
                _ => "others"
            };

            // 🔹 4. Create folder if not exists
            string folderPath = Path.Combine("wwwroot/uploads", folderName);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            // 🔹 5. Save encrypted file with .enc extension
            string savePath = Path.Combine(folderPath, file.FileName + ".enc");
            await System.IO.File.WriteAllTextAsync(savePath, encryptedData, cancellationToken);

            var response = ApiResponse<List<object>>.Success(new List<object> { new
            {
                message = "File encrypted and saved successfully.",
                folder = folderName,
                path = savePath.Replace("\\", "/")
            } }, "Success");

            return StatusCode(response.Code, response);
        }

        [HttpPost("upload-secure-with-thumb")]
        [BackOfficePermission(ModuleCodes.FILE_MANAGEMENT, PermissionCodes.FILE_UPLOAD)]
        public async Task<IActionResult> UploadSecureWithThumb(IFormFile file, string folderName = "images", CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
            {
                var errorResponse = ApiResponse<List<string>>.Fail("No file selected.", StatusCodes.BadRequest);
                return StatusCode(errorResponse.Code, errorResponse);
            }

            string ext = Path.GetExtension(file.FileName).ToLower();
            if (!IsImage(ext))
            {
                var errorResponse = ApiResponse<List<string>>.Fail("Only image files are supported for this endpoint.", StatusCodes.BadRequest);
                return StatusCode(errorResponse.Code, errorResponse);
            }

            // 🔹 1. Read original file into bytes
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms, cancellationToken);
            var originalBytes = ms.ToArray();

            // 🔹 2. Encrypt original
            string encryptedOriginal = _crypto.EncryptBytes(originalBytes);

            // 🔹 3. Generate Thumbnail using ImageSharp
            ms.Position = 0;
            using var image = await Image.LoadAsync(ms, cancellationToken);
            
            // Resize image (maintain aspect ratio)
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(300, 0), // Width 300, Height auto
                Mode = ResizeMode.Max
            }));

            using var thumbMs = new MemoryStream();
            await image.SaveAsJpegAsync(thumbMs, cancellationToken); // Save as Jpeg for thumb
            var thumbBytes = thumbMs.ToArray();

            // 🔹 4. Encrypt Thumbnail
            string encryptedThumb = _crypto.EncryptBytes(thumbBytes);

            // 🔹 5. Save both as .enc files
            string folderPath = Path.Combine("wwwroot/uploads", folderName);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string fileGuid = Guid.NewGuid().ToString();
            string mainSavePath = Path.Combine(folderPath, $"{fileGuid}_main{ext}.enc");
            string thumbSavePath = Path.Combine(folderPath, $"{fileGuid}_thumb{ext}.enc");

            await System.IO.File.WriteAllTextAsync(mainSavePath, encryptedOriginal, cancellationToken);
            await System.IO.File.WriteAllTextAsync(thumbSavePath, encryptedThumb, cancellationToken);

            var response = ApiResponse<List<object>>.Success(new List<object> { new
            {
                message = "Main and Thumbnail images encrypted and saved successfully.",
                mainPath = mainSavePath.Replace("\\", "/"),
                thumbPath = thumbSavePath.Replace("\\", "/")
            } }, "Success");

            return StatusCode(response.Code, response);
        }

        private bool IsImage(string ext)
        {
            return ext switch
            {
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" => true,
                _ => false
            };
        }


        [HttpGet("download/{fileName}")]
        [BackOfficePermission(ModuleCodes.FILE_MANAGEMENT, PermissionCodes.FILE_DOWNLOAD)]
        public async Task<IActionResult> DownloadDecryptedFile(string fileName, CancellationToken cancellationToken)
        {
            try
            {
                // 🔹 Detect folder based on file extension
                string ext = Path.GetExtension(fileName).ToLower();
                string folderName = ext switch
                {
                    ".jpg" or ".jpeg" or ".png" or ".gif" => "images",
                    ".mp4" or ".avi" or ".mov" => "videos",
                    ".pdf" => "pdfs",
                    ".doc" or ".docx" => "documents",
                    ".xls" or ".xlsx" => "excels",
                    _ => "others"
                };

                var encryptedPath = Path.Combine("wwwroot/uploads", folderName, fileName + ".enc");
                if (!System.IO.File.Exists(encryptedPath))
                {
                    var errorResponse = ApiResponse<List<string>>.Fail($"Encrypted file not found at path: {encryptedPath}", StatusCodes.NotFound);
                    return StatusCode(errorResponse.Code, errorResponse);
                }

                string encryptedData = await System.IO.File.ReadAllTextAsync(encryptedPath, cancellationToken);

                byte[] decryptedBytes = _crypto.DecryptBytes(encryptedData);

                string contentType = ext switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".mp4" => "video/mp4",
                    ".pdf" => "application/pdf",
                    ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    ".doc" => "application/msword",
                    ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    ".xls" => "application/vnd.ms-excel",
                    _ => "application/octet-stream"
                };

                return File(decryptedBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<List<string>>.Fail($"Error while decrypting and downloading file: {ex.Message}", StatusCodes.BadRequest);
                return StatusCode(errorResponse.Code, errorResponse);
            }
        }


        [HttpGet("stream/{fileName}")]
        [BackOfficePermission(ModuleCodes.FILE_MANAGEMENT, PermissionCodes.FILE_DOWNLOAD)]
        public async Task<IActionResult> StreamDecryptedVideo(string fileName, CancellationToken cancellationToken)
        {
            string ext = Path.GetExtension(fileName).ToLower();

            if (ext != ".mp4")
            {
                var errorResponse = ApiResponse<List<string>>.Fail("Only mp4 streaming supported.", StatusCodes.BadRequest);
                return StatusCode(errorResponse.Code, errorResponse);
            }

            string encryptedPath = Path.Combine("wwwroot/uploads/videos", fileName + ".enc");

            if (!System.IO.File.Exists(encryptedPath))
            {
                var errorResponse = ApiResponse<List<string>>.Fail($"Encrypted file not found: {encryptedPath}", StatusCodes.NotFound);
                return StatusCode(errorResponse.Code, errorResponse);
            }

            // Read encrypted file
            string encryptedData = await System.IO.File.ReadAllTextAsync(encryptedPath, cancellationToken);
            byte[] decryptedBytes = _crypto.DecryptBytes(encryptedData);

            // Convert decrypted bytes to memory stream
            var stream = new MemoryStream(decryptedBytes);

            return new FileStreamResult(stream, "video/mp4")
            {
                EnableRangeProcessing = true
            };
        }




    }
}
