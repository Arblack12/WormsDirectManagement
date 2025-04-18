namespace WormsDirectManagement.Models
{
    public class SenderRule
    {
        public string SenderEmail { get; set; } = "";
        public string FolderName { get; set; } = "";
        public string FileName { get; set; } = "";
        public int MonthOffset { get; set; }
        public int DayOffset { get; set; }
    }
}