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

        // GET: /Participant/Profile
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (HttpContext.Session.GetString("UserRole") != "Participant")
                return RedirectToAction("Login", "Auth");

            var response = await _httpClient.GetAsync("profile");
            var profile = new UserProfileViewModel();
            if (response.IsSuccessStatusCode)
            {
                profile = await response.Content.ReadFromJsonAsync<UserProfileViewModel>() ?? new UserProfileViewModel();
            }

            return View(profile);
        }

        // POST: /Participant/UploadPicture
        [HttpPost]
        public async Task<IActionResult> UploadPicture(UserProfileViewModel model)
        {
            if (HttpContext.Session.GetString("UserRole") != "Participant")
                return RedirectToAction("Login", "Auth");

            if (model.ProfileImage != null && model.ProfileImage.Length > 0)
            {
                using var form = new MultipartFormDataContent();
                using var stream = model.ProfileImage.OpenReadStream();
                var streamContent = new StreamContent(stream);
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(model.ProfileImage.ContentType);
                form.Add(streamContent, "file", model.ProfileImage.FileName);

                var response = await _httpClient.PostAsync("profile/picture", form);
                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Profile picture updated successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to upload image to Azure.";
                }
            }
            return RedirectToAction("Profile");
        }


    }

}
