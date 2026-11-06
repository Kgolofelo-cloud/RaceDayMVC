using Microsoft.AspNetCore.Mvc;
using RacedayMVC.Models;
using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;

namespace RacedayMVC.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly HttpClient _httpClient;

        public OrganiserController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }

        // GET: /Organiser/Index
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Auth");

            var response = await _httpClient.GetAsync("events/dashboard");
            var dashboardData = new DashboardSummaryViewModel();
            if (response.IsSuccessStatusCode)
            {
                dashboardData = await response.Content.ReadFromJsonAsync<DashboardSummaryViewModel>() ?? new DashboardSummaryViewModel();
            }

            return View(dashboardData);
        }

        // GET: /Organiser/Events
        public async Task<IActionResult> Events()
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Auth");

            var response = await _httpClient.GetAsync("events");
            var events = new List<EventViewModel>();
            if (response.IsSuccessStatusCode)
            {
                events = await response.Content.ReadFromJsonAsync<List<EventViewModel>>() ?? new List<EventViewModel>();
            }
            return View(events);
        }

        private async Task LoadLocationsIntoViewBag()
        {
            var response = await _httpClient.GetAsync("events/locations");
            if (response.IsSuccessStatusCode)
            {
                ViewBag.Locations = await response.Content.ReadFromJsonAsync<List<LocationViewModel>>();
            }
            else
            {
                ViewBag.Locations = new List<LocationViewModel>();
            }
        }

        // GET: /Organiser/CreateEvent
        [HttpGet]
        public async Task<IActionResult> CreateEvent()
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Auth");

            await LoadLocationsIntoViewBag();
            return View();
        }

        // POST: /Organiser/CreateEvent
        [HttpPost]
        public async Task<IActionResult> CreateEvent(CreateEventViewModel model)
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                await LoadLocationsIntoViewBag();
                return View(model);
            }

            // Strictly mapping to match the API's EventRequest schema and ignore the file in JSON
            var eventPayload = new
            {
                EventName = model.EventName,
                EventDate = model.EventDate.ToString("yyyy-MM-dd"),
                LocationID = model.LocationID
            };
            var content = new StringContent(JsonSerializer.Serialize(eventPayload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("events", content);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<EventResponse>();
                if (result != null && model.BannerImage != null && model.BannerImage.Length > 0)
                {
                    using var form = new MultipartFormDataContent();
                    using var stream = model.BannerImage.OpenReadStream();
                    var streamContent = new StreamContent(stream);
                    streamContent.Headers.ContentType = new MediaTypeHeaderValue(model.BannerImage.ContentType);
                    form.Add(streamContent, "file", model.BannerImage.FileName);

                    await _httpClient.PostAsync($"events/{result.EventID}/banner", form);
                }

                TempData["Success"] = "Event and banner successfully published!";
                return RedirectToAction("Events");
            }

            // Extract the exact 400 Bad Request error from the API if it fails again
            var errorResponse = await response.Content.ReadAsStringAsync();
            ViewBag.Error = $"API Error ({response.StatusCode}): {errorResponse}";
            await LoadLocationsIntoViewBag();
            return View(model);
        }

        // GET: /Organiser/AddCategory
        [HttpGet]
        public IActionResult AddCategory(int eventId)
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Auth");

            // Pass the EventID into the form so the API knows which event to attach it to
            var model = new CreateCategoryViewModel { EventID = eventId };
            return View(model);
        }

        // POST: /Organiser/AddCategory
        [HttpPost]
        public async Task<IActionResult> AddCategory(CreateCategoryViewModel model)
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid) return View(model);

            var payload = new { model.CategoryName, model.Distance, model.EntryFee };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            // Send to the API's specific category endpoint
            var response = await _httpClient.PostAsync($"events/{model.EventID}/categories", content);
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Race category added successfully!";
                return RedirectToAction("Events");
            }

            var errorResponse = await response.Content.ReadAsStringAsync();
            ViewBag.Error = $"API Error ({response.StatusCode}): {errorResponse}";
            return View(model);
        }

        // GET: /Organiser/ViewEnrolments
        [HttpGet]
        public async Task<IActionResult> ViewEnrolments(int eventId, string eventName)
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Auth");

            ViewBag.EventName = eventName;
            ViewBag.EventID = eventId;

            var response = await _httpClient.GetAsync($"registrations/event/{eventId}");
            var participants = new List<EventParticipantViewModel>();

            if (response.IsSuccessStatusCode)
            {
                participants = await response.Content.ReadFromJsonAsync<List<EventParticipantViewModel>>() ?? new List<EventParticipantViewModel>();
            }

            return View(participants);
        }

        // POST: /Organiser/PublishResult
        [HttpPost]
        public async Task<IActionResult> PublishResult(int eventId, string eventName, int registrationId, string finishTime, int position)
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Auth");

            var payload = new { RegistrationID = registrationId, FinishTime = finishTime, OverallPosition = position };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("results", content);

            if (response.IsSuccessStatusCode)
                TempData["Success"] = "Result published successfully!";
            else
                TempData["Error"] = "Failed to publish result. Format must be HH:MM:SS.";

            return RedirectToAction("ViewEnrolments", new { eventId = eventId, eventName = eventName });
        }

    }
}