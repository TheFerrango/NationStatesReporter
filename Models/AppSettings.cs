namespace nsreporter
{
    public class AppSettings
    {
        public string Nation { get; set; }
        public string Password { get; set; }
        public int DayOfMonth { get; set; }
        public bool MakeProgressReport { get; set; }
        public string ReportTitle { get; set; }
        public string TemplateFile { get; set; }
    }
}