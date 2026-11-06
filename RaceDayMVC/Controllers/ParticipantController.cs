using Microsoft.AspNetCore.Mvc;

using RacedayMVC.Models;

using System.Text.Json;

using System.Text;

namespace RacedayMVC.Controllers

{

    public class ParticipantController : Controller

    {

        private readonly HttpClient _httpClient;

        public ParticipantController(IHttpClientFactory httpClientFactory)

        {

            _httpClient = httpClientFactory.CreateClient("ApiClient");

        }

        // GET: /Participant/Index

        public async Task<IActionResult> Index()

        {

            if (HttpContext.Session.GetString("UserRole") != "Participant")

                return RedirectToAction("Login", "Auth");

            var response = await _httpClient.GetAsync("events");

            var events = new List<EventWithCategoriesViewModel>();

            if (response.IsSuccessStatusCode)

            {

                events = await response.Content.ReadFromJsonAsync<List<EventWithCategoriesViewModel>>() ?? new List<EventWithCategoriesViewModel>();

            }

            return View(events);

        }

        // POST: /Participant/Enrol

        [HttpPost]

        public async Task<IActionResult> Enrol(int categoryId)

        {

            if (HttpContext.Session.GetString("UserRole") != "Participant")

                return RedirectToAction("Login", "Auth");

            var payload = new { CategoryID = categoryId };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("registrations", content);

            if (response.IsSuccessStatusCode)

            {

                TempData["Success"] = "Successfully enrolled in the race!";

            }

            else

            {

                var error = await response.Content.ReadAsStringAsync();

                TempData["Error"] = $"Enrolment failed: {error}";

            }

            return RedirectToAction("Index");

        }

        // GET: /Participant/MyEvents
        [HttpGet]
        public async Task<IActionResult> MyEvents()
        {
            if (HttpContext.Session.GetString("UserRole") != "Participant")
                return RedirectToAction("Login", "Auth");

            // Assuming your API endpoint is GET: api/v1/registrations
            var response = await _httpClient.GetAsync("registrations");
            var enrolments = new List<MyEnrolmentViewModel>();
            if (response.IsSuccessStatusCode)
            {
                enrolments = await response.Content.ReadFromJsonAsync<List<MyEnrolmentViewModel>>() ?? new List<MyEnrolmentViewModel>();
            }
            return View(enrolments);
        }

        // GET: /Participant/Results
        [HttpGet]
        public async Task<IActionResult> Results()
        {
            if (HttpContext.Session.GetString("UserRole") != "Participant")
                return RedirectToAction("Login", "Auth");

            // Assuming your API endpoint is GET: api/v1/results
            var response = await _httpClient.GetAsync("results");
            var results = new List<MyResultViewModel>();
            if (response.IsSuccessStatusCode)
            {
                results = await response.Content.ReadFromJsonAsync<List<MyResultViewModel>>() ?? new List<MyResultViewModel>();
            }
            return View(results);
        }

    }

}
