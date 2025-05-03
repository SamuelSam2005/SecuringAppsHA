using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SecureDocumentExchange.Web.Models;
using System.Security.Claims;

namespace SecureDocumentExchange.Web.Controllers
{
    public class AccountController : Controller
    {
        private static List<User> dummyUsers = new()
        {
            new User { Email = "lawyer@example.com", FirstName = "John", LastName = "Doe" },
            new User { Email = "client@example.com", FirstName = "Alice", LastName = "Smith" }
        };

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var user = dummyUsers.FirstOrDefault(u =>
                u.Email == model.Email &&
                u.FirstName == model.FirstName &&
                u.LastName == model.LastName);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid credentials");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim("FirstName", user.FirstName),
                new Claim("LastName", user.LastName)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
