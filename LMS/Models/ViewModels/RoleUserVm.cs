using System.ComponentModel.DataAnnotations;

namespace LMS.Models.ViewModels
{
    public class RoleUserVm
    {
        [Display(Name = "User")]
        public required string UserId { get; set; }
        [Display(Name = "Role")]
        public required string RoleId { get; set; }
    }
}
