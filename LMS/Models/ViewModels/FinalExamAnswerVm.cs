namespace LMS.Models.ViewModels
{
    public class FinalExamAnswerVm
    {
        public int ExamId { get; set; }
        public int SubmissionId { get; set; }
        public Dictionary<int, string> Answers { get; set; } = new();
    }
}
