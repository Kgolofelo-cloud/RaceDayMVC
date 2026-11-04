using Microsoft.AspNetCore.Mvc;

namespace RacedayMVC.Controllers
{
    public class OrganiserController : Controller
    {
        public IActionResult Index()
        {
            // Block unauthenticated access
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Auth");

            return View();
        }
    }
}