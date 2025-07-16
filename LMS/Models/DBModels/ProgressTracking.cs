namespace LMS.Models.DBModels
{
    public class ProgressTracking
    {
        public int Id { get; set; }
        public required string StudentId { get; set; }
        public int? CourseId { get; set; }
        public int? ModuleId { get; set; }
        public bool Completed { get; set; }
        public DateTime CompletionDate { get; set; }
        public Course? Course { get; set; }
        public Module? Module { get; set; }
    }
}
