namespace LMS.Models.ViewModels
{
    public class CertificateVm
    {
        public string StudentName { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public DateTime CompletionDate { get; set; }
    }
}
