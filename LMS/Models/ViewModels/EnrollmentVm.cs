using LMS.Models.DBModels;

namespace LMS.Models.ViewModels
{
    public class EnrollmentVm
    {
        public int Id { get; set; }
        public string StudentId { get; set; }
        public int CourseId { get; set; }
        public string? CourseName { get; set; }
        public decimal CoursePrice { get; set; }
        public int CourseDuaration { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public List<ProgressTracking>? ProgressTrackings { get; set; }
        public double ProgressPercent { get; set; }

        public bool IsFinalExamPassed { get; set; }
    }
}
