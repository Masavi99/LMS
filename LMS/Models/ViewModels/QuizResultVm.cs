namespace LMS.Models.ViewModels
{
    public class QuizResultVm
    {
        public int CourseId { get; set; }
        public int ModuleId { get; set; }
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public bool Passed { get; set; }
        public List<StudentAnswerVm>? StudentAnswers { get; set; }
    }
}
