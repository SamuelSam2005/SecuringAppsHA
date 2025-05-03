using System.ComponentModel.DataAnnotations;

namespace SecureDocumentExchange.Web.Models
{
    public class FileUploadViewModel
    {
        [Required]
        public string LawyerEmail { get; set; }

        [Required]
        public IFormFile File { get; set; }
    }
}
