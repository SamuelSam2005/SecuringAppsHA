using System.Security.Cryptography;
using System.Text;

namespace SecureDocumentExchange.Web.Services
{
    public static class FileSignerService
    {
        public static byte[] GenerateSignature(byte[] fileBytes, string privateKeyPath)
        {
            var privateKeyPem = File.ReadAllText(privateKeyPath);
            using var rsa = RSA.Create();
            rsa.ImportFromPem(privateKeyPem.ToCharArray());

            var signature = rsa.SignData(fileBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            return signature;
        }
    }
}
