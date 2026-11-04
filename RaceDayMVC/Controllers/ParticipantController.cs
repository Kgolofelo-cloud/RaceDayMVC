using Microsoft.AspNetCore.Mvc;

namespace RacedayMVC.Controllers

{

    public class ParticipantController : Controller

    {

        public IActionResult Index()

        {

            // Block unauthenticated access

            if (HttpContext.Session.GetString("UserRole") != "Participant")

                return RedirectToAction("Login", "Auth");

            return View();

        }

    }

}
