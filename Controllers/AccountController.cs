using BurgerJoint.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Cryptography;

namespace BurgerJoint.Controllers
{
    public class AccountController : Controller
    {
        //  IN-MEMORY STORE (replace with DB if you want persistence)
        private static readonly Dictionary<string, string> _users = new(); // email -> hash

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                ModelState.AddModelError("", "Email and password required.");

            if (ModelState.IsValid && _users.ContainsKey(email) && Verify(password, _users[email]))
            {
                var claims = new List<Claim> { new Claim(ClaimTypes.Name, email) };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
                return RedirectToAction("Create", "Burger"); // send straight to “Add Burger”
            }
            ModelState.AddModelError("", "Invalid login.");
            return View();
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(string email, string password, string confirmPassword)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                ModelState.AddModelError("", "All fields required.");
            if (password != confirmPassword)
                ModelState.AddModelError("", "Passwords do not match.");
            if (_users.ContainsKey(email))
                ModelState.AddModelError("", "Email already registered.");

            if (ModelState.IsValid)
            {
                _users[email] = Hash(password);
                return RedirectToAction(nameof(Login));
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Burger");
        }

        //  HASH HELPERS
        private static string Hash(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
        private static bool Verify(string password, string hash) => Hash(password) == hash;
    }
}