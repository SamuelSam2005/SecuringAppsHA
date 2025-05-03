using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureDocumentExchange.Web.Models;
using SecureDocumentExchange.Web.Helpers;
using SecureDocumentExchange.Web.Filters;

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

        [HttpGet]
        [VerifyAccessFilter]
        public IActionResult Download(string file)
        {
            var secureFolder = Path.Combine(_env.ContentRootPath, "SecureFiles");
            var filePath = Path.Combine(secureFolder, file);

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var mime = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            return PhysicalFile(filePath, mime, file);
        }
    }
}
