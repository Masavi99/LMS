namespace LMS.Models.DBModels
{
    public class Quiz
    {
        public int Id { get; set; }
        public int ModuleId { get; set; }
        public required string Title { get; set; }

        public Module? Module { get; set; }

        public List<QuizQuestion>? QuizQuestions { get; set; }
    }
}
