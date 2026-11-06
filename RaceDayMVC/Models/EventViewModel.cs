using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RacedayMVC.Models
{
    public class EventViewModel
    {
        public int EventID { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string? BannerUrl { get; set; }
    }

    public class CreateEventViewModel
    {
        [Required(ErrorMessage = "Event name is required")]
        public string EventName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Date is required")]
        public DateTime EventDate { get; set; } = DateTime.Now.AddDays(30);
        [Required(ErrorMessage = "Please select a location")]
        public int LocationID { get; set; }

        public IFormFile? BannerImage { get; set; }
    }

    public class EventResponse
    {
        public int EventID { get; set; }
    }

    public class LocationViewModel
    {
        public int LocationID { get; set; }
        public string DisplayName { get; set; } = string.Empty;
    }

    public class CategoryViewModel
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal Distance { get; set; }
        public decimal EntryFee { get; set; }
    }

    public class EventWithCategoriesViewModel : EventViewModel
    {
        public List<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();
    }


}