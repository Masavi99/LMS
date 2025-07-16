using LMS.Models.DBModels;

namespace LMS.Models.ViewModels
{
    public class FinalExamResultVm
    {
        public int CourseId { get; set; }
        public string? ExamTitle { get; set; }
        public string? ExamType { get; set; } // "MCQ" or "Written"
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public bool Passed { get; set; }
        public DateTime SubmissionDate { get; set; }
        public string? WrittenSubmissionUrl { get; set; }
        public List<StudentAnswerVm>? Answers { get; set; } = new();

        public FinalExam? Exam { get; set; }
        public FinalExamSubmission? Submission { get; set; }
        public List<StudentFinalExamAnswer>? StudentFinalExamAnswer { get; set; } = new();
    }
}
