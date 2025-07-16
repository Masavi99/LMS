namespace LMS.Models.DBModels
{
    public class Module
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public required string Title { get; set; }
        public string? ContentType { get; set; }
        public string? Contents { get; set; }
        public int ModuleOrder { get; set; }
        public Course? Course { get; set; }
        public ProgressTracking? ProgressTracking { get; set; }
    }
}
