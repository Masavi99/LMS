namespace LMS.Models.DBModels
{
    public class FinalExamQuestion
    {
        public int Id { get; set; }
        public int FinalExamId { get; set; }
        public required string Question { get; set; }
        public string? OptionA { get; set; }
        public string? OptionB { get; set; }
        public string? OptionC { get; set; }
        public string? OptionD { get; set; }
        public string? CorrectOption { get; set; }
        public int? Marks { get; set; }
    }
}
