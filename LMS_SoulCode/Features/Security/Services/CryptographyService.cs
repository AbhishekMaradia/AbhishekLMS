using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace LMS_SoulCode.Features.Security.Services
{
    public class CryptographyService
    {
        private readonly byte[] _key;

        public CryptographyService(IConfiguration config)
        {
            // Appsettings ya environment me key rakho (base64 me)
            var keyString = config["CryptoSettings:KeyBase64"];
            _key = Convert.FromBase64String(keyString ?? throw new Exception("Missing key"));
        }

        // 🔸 String encrypt karne ke liye
        public string Encrypt(string plainText)
        {
            var bytes = Encoding.UTF8.GetBytes(plainText);
            return EncryptBytes(bytes);
        }

        // 🔸 String decrypt karne ke liye
        public string Decrypt(string encryptedText)
        {
            var bytes = DecryptBytes(encryptedText);
            return Encoding.UTF8.GetString(bytes);
        }

        // 🔸 Dynamic object encrypt
        public string EncryptDynamic(object data)
        {
            string json = JsonSerializer.Serialize(data);
            return Encrypt(json);
        }

        // 🔸 Dynamic decrypt (return object)
        public T DecryptDynamic<T>(string encryptedText)
        {
            string json = Decrypt(encryptedText);
            return JsonSerializer.Deserialize<T>(json)!;
        }

        // Internal AES-GCM encrypt bytes
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

        /// <summary>
        /// Decrypts a large encrypted string (Base64) directly into a result stream.
        /// This is still limited by the fact that AesGcm needs the full cipher block,
        /// but it avoids keeping multiple copies of the huge byte arrays in memory.
        /// </summary>
        public async Task DecryptToStreamAsync(string encryptedBase64, Stream outputStream, CancellationToken ct = default)
        {
            // Convert Base64 to Stream to process without loading whole string as bytes if possible
            // But Convert.FromBase64String already creates a byte array.
            // Best we can do with current AesGcm block storage is to decrypt and write in chunks to output
            byte[] plainBytes = DecryptBytes(encryptedBase64);
            
            const int bufferSize = 81920; // 80KB chunks
            for (int i = 0; i < plainBytes.Length; i += bufferSize)
            {
                int count = Math.Min(bufferSize, plainBytes.Length - i);
                await outputStream.WriteAsync(plainBytes, i, count, ct);
            }
        }
    }
}
