using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureDocumentExchange.Web.Models;
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
                ModelState.AddModelError("", "Invalid file");
                return View(model);
            }

            // Generate secure access code
            string accessCode = Guid.NewGuid().ToString();

            // Secure storage path (outside wwwroot)
            string secureFolder = Path.Combine(_env.ContentRootPath, "SecureFiles");
            if (!Directory.Exists(secureFolder))
                Directory.CreateDirectory(secureFolder);

            string fileName = Path.GetFileName(model.File.FileName);
            string savePath = Path.Combine(secureFolder, fileName);

            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                await model.File.CopyToAsync(stream);
            }

            TempData["Message"] = $"File uploaded successfully. Access Code: {accessCode}";
            TempData["LawyerEmail"] = model.LawyerEmail;

            // Log metadata for now (will handle logging separately later)
            System.IO.File.WriteAllText(Path.Combine(secureFolder, $"{fileName}.meta.txt"),
                $"AccessCode: {accessCode}\nLawyerEmail: {model.LawyerEmail}\nUploader: {User.Identity.Name}");

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
