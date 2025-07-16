namespace LMS.Models.DBModels
{
    public class Enrollments
    {
        public int Id { get; set; }
        public required string StudentId { get; set; }
        public int CourseId { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public bool IsCompleted { get; set; }
        public Course? Course { get; set; }
    }
}
