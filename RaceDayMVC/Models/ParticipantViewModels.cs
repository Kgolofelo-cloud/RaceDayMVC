namespace RacedayMVC.Models
{
    public class MyEnrolmentViewModel
    {
        public int RegistrationID { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal Distance { get; set; }
        public DateTime RegistrationDate { get; set; }
    }

    public class MyResultViewModel
    {
        public string EventName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string FinishTime { get; set; } = string.Empty;
        public int OverallPosition { get; set; }
    }
}