using LMS.Models.DBModels;

namespace LMS.Models.ViewModels
{
    public class FinalExamVm
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string? CourseTitle { get; set; }
        public string? Title { get; set; }
        public int ExamId { get; set; }
        public string? Question { get; set; }
        public string? ExamType { get; set; }
        public string? OptionA { get; set; }
        public string? OptionB { get; set; }
        public string? OptionC { get; set; }
        public string? OptionD { get; set; }
        public string? CorrectOption { get; set; }
        public int StudentId { get; set; }
        public int Score { get; set; }
        public int Passed { get; set; }
        public int? Marks { get; set; }
        public int MaxAttempts { get; set; }
        public DateTime AttemptDate { get; set; }
        public List<FinalExamQuestion>? FinalExamQuestion { get; set; }
    }

    public class FinalExamStatusVm
    {
        public FinalExam? Exam { get; set; }
        public bool HasPassed { get; set; }
        public string? UserId { get; set; }
    }

}
