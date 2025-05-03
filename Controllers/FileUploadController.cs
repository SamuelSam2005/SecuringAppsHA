using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureDocumentExchange.Web.Models;
using SecureDocumentExchange.Web.Helpers;
using SecureDocumentExchange.Web.Filters;
using SecureDocumentExchange.Web.Services;

namespace SecureDocumentExchange.Web.Controllers
{
    [Authorize]
    public class FileUploadController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public FileUploadController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpGet]
        public IActionResult Upload() => View();

        [HttpPost]
        public async Task<IActionResult> Upload(FileUploadViewModel model)
        {
            if (!ModelState.IsValid || model.File == null || model.File.Length == 0)
            {
                ModelState.AddModelError("", "Invalid file.");
                return View(model);
            }

            // Header-based validation: allow only true .docx
            if (!FileValidator.IsValidDocx(model.File))
            {
                ModelState.AddModelError("", "Only real .docx files are allowed.");
                return View(model);
            }

            // Sanitize input to avoid XSS injection
            var lawyerEmail = Sanitizer.Clean(model.LawyerEmail);

            // Generate secure access code
            string accessCode = Guid.NewGuid().ToString();

            // Define secure (non-wwwroot) storage path
            string secureFolder = Path.Combine(_env.ContentRootPath, "SecureFiles");
            if (!Directory.Exists(secureFolder))
                Directory.CreateDirectory(secureFolder);

            // Save file
            string fileName = Path.GetFileName(model.File.FileName);
            string savePath = Path.Combine(secureFolder, fileName);

            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                await model.File.CopyToAsync(stream);
            }

            // Save metadata (access control)
            string metaPath = Path.Combine(secureFolder, $"{fileName}.meta.txt");
            System.IO.File.WriteAllText(metaPath,
                $"AccessCode: {accessCode}\nLawyerEmail: {lawyerEmail}\nUploader: {User.Identity.Name}");

            TempData["Message"] = $"File uploaded successfully. Access Code: {accessCode}";
            TempData["LawyerEmail"] = lawyerEmail;

            return RedirectToAction("Upload");
        }

        [HttpPost]
        [VerifyAccessFilter]
        [HttpPost]
        [VerifyAccessFilter]
        public IActionResult Download(string file, string email, string code, string publicKey)
        {
            var secureFolder = Path.Combine(_env.ContentRootPath, "SecureFiles");
            var filePath = Path.Combine(secureFolder, file);

            if (!System.IO.File.Exists(filePath))
                return NotFound("File not found.");

            // Read the original file
            var fileBytes = System.IO.File.ReadAllBytes(filePath);

            // 🔐 1. Encrypt the file using AES
            var (encryptedFile, aesKey, iv) = HybridEncryptionService.EncryptFile(fileBytes);

            // 🔐 2. Encrypt the AES key using the lawyer's public key
            var encryptedAesKey = HybridEncryptionService.EncryptAesKeyWithRsa(aesKey, publicKey);

            // 🖊️ 3. Sign the encrypted file using the server's private key
            string privateKeyPath = Path.Combine(_env.ContentRootPath, "signing_private.pem");
            var signature = FileSignerService.GenerateSignature(encryptedFile, privateKeyPath);

            // 📦 4. Build a ZIP containing: EncryptedFile, AES Key, IV, Signature
            using var memoryStream = new MemoryStream();
            using (var archive = new System.IO.Compression.ZipArchive(memoryStream, System.IO.Compression.ZipArchiveMode.Create, true))
            {
                var encFile = archive.CreateEntry("EncryptedFile.docx");
                using (var entryStream = encFile.Open())
                    entryStream.Write(encryptedFile, 0, encryptedFile.Length);

                var keyFile = archive.CreateEntry("EncryptedAESKey.txt");
                using (var entryStream = keyFile.Open())
                    entryStream.Write(encryptedAesKey, 0, encryptedAesKey.Length);

                var ivFile = archive.CreateEntry("IV.txt");
                using (var entryStream = ivFile.Open())
                    entryStream.Write(iv, 0, iv.Length);

                var signatureFile = archive.CreateEntry("Signature.txt");
                using (var entryStream = signatureFile.Open())
                    entryStream.Write(signature, 0, signature.Length);
            }

            // 🎯 5. Return the ZIP file
            return File(memoryStream.ToArray(), "application/zip", "EncryptedPackage.zip");
        }


    }
}
