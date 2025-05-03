using System.ComponentModel.DataAnnotations;

namespace SecureDocumentExchange.Web.Models
{
    public class LoginViewModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }
    }
}
