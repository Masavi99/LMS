namespace LMS.Models.DBModels
{
    public class QuizResult
    {
        public int Id { get; set; }
        public required string StudentId { get; set; }
        public int QuizId { get; set; }
        public int Score { get; set; }
        public bool Passed { get; set; }
        public DateTime AttemptDate { get; set; }
        public Quiz? Quiz { get; set; }
    }
}
