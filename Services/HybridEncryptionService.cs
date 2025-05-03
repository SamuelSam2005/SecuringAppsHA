using System.Security.Cryptography;
using System.Text;

namespace SecureDocumentExchange.Web.Services
{
    public class HybridEncryptionService
    {
        public static (byte[] EncryptedData, byte[] Key, byte[] IV) EncryptFile(byte[] fileBytes)
        {
            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.GenerateKey();
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            var encrypted = encryptor.TransformFinalBlock(fileBytes, 0, fileBytes.Length);

            return (encrypted, aes.Key, aes.IV);
        }

        public static byte[] EncryptAesKeyWithRsa(byte[] aesKey, string base64PublicKey)
        {
            var publicKeyBytes = Convert.FromBase64String(base64PublicKey);

            using var rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);

            return rsa.Encrypt(aesKey, RSAEncryptionPadding.OaepSHA256);
        }
    }
}
