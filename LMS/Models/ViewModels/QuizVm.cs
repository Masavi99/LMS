using LMS.Models.DBModels;

namespace LMS.Models.ViewModels
{
    public class QuizVm
    {
        public int Id { get; set; }
        public int ModuleId { get; set; }
        public string? Title { get; set; }
        public string? CourseTitle { get; set; }
        public string? ModuleTitle { get; set; }
        public int QuizId { get; set; }
        public string? Question { get; set; }
        public string? OptionA { get; set; }
        public string? OptionB { get; set; }
        public string? OptionC { get; set; }
        public string? OptionD { get; set; }
        public string? CorrectOption { get; set; }
        public int StudentId { get; set; }
        public int Score { get; set; }
        public int Passed { get; set; }
        public DateTime AttemptDate { get; set; }
        public List<QuizQuestion>? QuizQuestion { get; set; }
    }
}
