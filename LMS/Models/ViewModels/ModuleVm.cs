namespace LMS.Models.ViewModels
{
    public class ModuleVm
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string? Title { get; set; }
        public int ModuleOrder { get; set; }
        public string? Contents { get; set; }
        public bool IsUnlocked { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsPassed { get; set; }
    }
}
