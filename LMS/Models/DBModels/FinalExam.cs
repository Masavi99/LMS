namespace LMS.Models.DBModels
{
    public class FinalExam
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public required string Title { get; set; }
        public required string ExamType { get; set; }
        public int PassingScore { get; set; }
        public int MaxAttempts { get; set; }
        public int Duration { get; set; }
        public Course? Course { get; set; }
        public FinalExamQuestion? FinalExamQuestion { get; set; }
        public List<FinalExamSubmission>? FinalExamSubmission { get; set; }
    }
}
