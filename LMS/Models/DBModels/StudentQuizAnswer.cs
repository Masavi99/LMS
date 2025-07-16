namespace LMS.Models.DBModels
{
    public class StudentQuizAnswer
    {
        public int Id { get; set; }
        public int QuizResultId { get; set; }
        public int QuizQuestionId { get; set; }
        public required string SelectedOption { get; set; }
        public bool IsCorrect { get; set; }

        public QuizQuestion? QuizQuestion { get; set; }

    }
}
