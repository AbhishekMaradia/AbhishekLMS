using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Microsoft.Extensions.Configuration;

namespace LMS_SoulCode.Features.Security.Services
{
    public class CryptographyService
    {
        private readonly byte[] _key;
        public byte[] Key => _key;
        private readonly IConfiguration _config;

        public CryptographyService(IConfiguration config)
        {
            _config = config;
            var keyString = config["CryptoSettings:KeyBase64"];
            _key = Convert.FromBase64String(keyString ?? throw new Exception("Missing key"));
        }

        // 🔸 Helper for Image Resizing (Standardized)
        public async Task<byte[]> ProcessImageAsync(Stream stream, int maxSize, CancellationToken ct)
        {
            stream.Position = 0;
            using var image = await Image.LoadAsync(stream, ct);
            
            // Resize logic: only if original is larger than maxSize
            if (image.Width > maxSize || image.Height > maxSize)
            {
                image.Mutate(x => x.Resize(new ResizeOptions { Size = new Size(maxSize, maxSize), Mode = ResizeMode.Max }));
            }
            
            using var outMs = new MemoryStream();
            await image.SaveAsJpegAsync(outMs, ct);
            return outMs.ToArray();
        }

        public async Task<(byte[] Main, byte[] Thumb)> ProcessImageWithThumbAsync(Stream stream, int thumbSize, CancellationToken ct)
        {
            // For Main Image: Use int.MaxValue to keep original size
            var main = await ProcessImageAsync(stream, int.MaxValue, ct);
            
            // For Thumbnail: Use the size passed by the caller
            var thumb = await ProcessImageAsync(stream, thumbSize, ct);
            
            return (main, thumb);
        }

        // 🔸 Binary Encryption (No Base64 - Saves 33% space)
        public async Task EncryptLargeFileAsync(Stream inputStream, string outputPath, CancellationToken ct = default)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV();

            using var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await fileStream.WriteAsync(aes.IV, 0, aes.IV.Length, ct);

            using var encryptor = aes.CreateEncryptor();
            using var cryptoStream = new CryptoStream(fileStream, encryptor, CryptoStreamMode.Write);
            
            await inputStream.CopyToAsync(cryptoStream, ct);
        }

        public Stream GetDecryptStream(string inputPath)
        {
            var fileStream = new FileStream(inputPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            
            byte[] iv = new byte[16];
            fileStream.Read(iv, 0, iv.Length);

            var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = iv;

            var decryptor = aes.CreateDecryptor();
            return new CryptoStream(fileStream, decryptor, CryptoStreamMode.Read);
        }

        public async Task DecryptLargeFileToStreamAsync(string inputPath, Stream outputStream, CancellationToken ct = default)
        {
            using var fileStream = new FileStream(inputPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            
            byte[] iv = new byte[16];
            await fileStream.ReadAsync(iv, 0, iv.Length, ct);

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            using var cryptoStream = new CryptoStream(fileStream, decryptor, CryptoStreamMode.Read);

            await cryptoStream.CopyToAsync(outputStream, ct);
        }

        public string EncryptBytes(byte[] plainBytes)
        {
            byte[] iv = new byte[12];
            RandomNumberGenerator.Fill(iv);

            byte[] cipher = new byte[plainBytes.Length];
            byte[] tag = new byte[16];

            using var aes = new AesGcm(_key);
            aes.Encrypt(iv, plainBytes, cipher, tag);

            byte[] combined = new byte[iv.Length + tag.Length + cipher.Length];
            Buffer.BlockCopy(iv, 0, combined, 0, iv.Length);
            Buffer.BlockCopy(tag, 0, combined, iv.Length, tag.Length);
            Buffer.BlockCopy(cipher, 0, combined, iv.Length + tag.Length, cipher.Length);

            return Convert.ToBase64String(combined);
        }

        public byte[] DecryptBytes(string encrypted)
        {
            byte[] combined = Convert.FromBase64String(encrypted);
            byte[] iv = new byte[12];
            byte[] tag = new byte[16];
            byte[] cipher = new byte[combined.Length - iv.Length - tag.Length];

            Buffer.BlockCopy(combined, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(combined, iv.Length, tag, 0, tag.Length);
            Buffer.BlockCopy(combined, iv.Length + tag.Length, cipher, 0, cipher.Length);

            byte[] plain = new byte[cipher.Length];
            using var aes = new AesGcm(_key);
            aes.Decrypt(iv, cipher, tag, plain);
            return plain;
        }

        public string EncryptDynamic<T>(T data)
        {
            var json = JsonSerializer.Serialize(data);
            return EncryptBytes(Encoding.UTF8.GetBytes(json));
        }

        public T? DecryptDynamic<T>(string encrypted)
        {
            var bytes = DecryptBytes(encrypted);
            var json = Encoding.UTF8.GetString(bytes);
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}
