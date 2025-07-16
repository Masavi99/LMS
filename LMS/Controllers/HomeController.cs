using LMS.Data;
using LMS.Models;
using LMS.Models.DBModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Diagnostics;
using System.Security.Claims;
using X.PagedList.Extensions;

namespace LMS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private ApplicationDbContext _db;
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index(int? page)
        {
            ViewBag.Roles = HttpContext.Session.GetString("UserRoles");
            var courses = _db.Course.ToList();

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var enrolledCourseIds = new List<int>();

            if (!string.IsNullOrEmpty(userId))
            {
                enrolledCourseIds = _db.Enrollments.Where(e => e.StudentId == userId && e.IsCompleted == true).Select(e => e.CourseId).ToList();
            }

            ViewBag.EnrolledCourses = enrolledCourseIds;

            return View(courses.ToPagedList(page ?? 1, 10));
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
