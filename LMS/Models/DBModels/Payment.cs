using Microsoft.EntityFrameworkCore;

namespace LMS.Models.DBModels
{
    public class Payment
    {
        public int Id { get; set; }
        public required string StudentId { get; set; }
        public int EnrollmentsId { get; set; }
        [Precision(18, 2)]
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public bool PaymentStatus { get; set; }
        public Enrollments? Enrollments { get; set; }
    }
}
