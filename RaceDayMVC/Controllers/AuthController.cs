using Microsoft.AspNetCore.Mvc;
using RacedayMVC.Models;
using System.Text.Json;
using System.Text;

namespace RacedayMVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly HttpClient _httpClient;

        public AuthController(IHttpClientFactory httpClientFactory)
        {
            // This uses the exact BaseAddress we set up in Program.cs
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Package the data and send it to the Part 2 API
            var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("Auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                // Read the API response to get the user's Role
                var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
                if (result != null)
                {
                    // Store the authenticated state in the MVC Session
                    HttpContext.Session.SetString("UserEmail", model.Email);
                    HttpContext.Session.SetString("UserRole", result.Role);

                    // Redirect to distinct interfaces based on role
                    if (result.Role == "Organiser")
                        return RedirectToAction("Index", "Organiser");
                    else
                        return RedirectToAction("Index", "Participant");
                }
            }

            // Handle invalid inputs gracefully without crashing (Rubric requirement)
            ViewBag.Error = "Invalid email or password.";
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("Auth/register", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Account created successfully! Please log in.";
                return RedirectToAction("Login");
            }

            ViewBag.Error = "Registration failed. Ensure your email is unique and details are correct.";
            return View(model);
        }

        public IActionResult Logout()
        {
            // End session and redirect
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}