namespace LMS.Models.DBModels
{
    public class FinalExamSubmission
    {
        public int Id { get; set; }
        public int FinalExamId { get; set; }
        public required string StudentId { get; set; }
        public int Score { get; set; }
        public bool Passed { get; set; }
        public int Attempts { get; set; }
        public DateTime SubmissionDate { get; set; }

        public FinalExam? FinalExam { get; set; }
        public ApplicationUser? Student { get; set; }

    }
}
