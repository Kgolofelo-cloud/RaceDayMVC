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

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Auth");

            return View();
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
    }
}