namespace LMS.Models.DBModels
{
    public class StudentFinalExamAnswer
    {
        public int Id { get; set; }

        public int FinalExamSubmissionId { get; set; }
        public FinalExamSubmission? Submission { get; set; }

        public int? FinalExamQuestionId { get; set; }
        public FinalExamQuestion? FinalExamQuestion { get; set; }

        public string? SelectedOption { get; set; }
        public bool IsCorrect { get; set; }
        public string? WrittenSubmissionURL { get; set; }
    }
}
