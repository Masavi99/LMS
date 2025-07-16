using Microsoft.EntityFrameworkCore;

namespace LMS.Models.DBModels
{
    public class Course
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public string? Duration { get; set; }
        public string? ImageUrl { get; set; }
        public string? IntroVideoUrl { get; set; }
        [Precision(18, 2)]
        public decimal Price { get; set; }
        public bool IsPaid { get; set; }
    }
}
