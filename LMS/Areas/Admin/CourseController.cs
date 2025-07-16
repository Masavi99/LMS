using LMS.Data;
using LMS.Models.DBModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LMS.Areas.Admin
{
    [Area("Admin")]
    public class CourseController : Controller
    {
        private ApplicationDbContext _db;
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment _he;
        public CourseController(ApplicationDbContext db, Microsoft.AspNetCore.Hosting.IHostingEnvironment he)
        {
            _db = db;
            _he = he;
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            return View(_db.Course.ToList());
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Course course, IFormFile image)
        {
            if (ModelState.IsValid)
            {
                var searchProduct = _db.Course.FirstOrDefault(c => c.Title == course.Title);
                if (searchProduct != null)
                {
                    ViewBag.message = "This Course is already exist";
                    return View(course);
                }
                if (image != null)
                {
                    var name = Path.Combine(_he.WebRootPath + "/Images", Path.GetFileName(image.FileName));
                    await image.CopyToAsync(new FileStream(name, FileMode.Create));
                    course.ImageUrl = "Images/" + image.FileName;
                }
                if (image == null)
                {
                    course.ImageUrl = "Images/noimage.png";
                }
                _db.Course.Add(course);
                await _db.SaveChangesAsync();
                TempData["save"] = "Course Saved Successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(course);
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id)
        {
            var course = _db.Course.FirstOrDefault(x => x.Id == id);
            return View(course);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Course course, IFormFile image)
        {
            ModelState.Remove("Image");
            if (ModelState.IsValid)
            {
                var prevCourse = _db.Course.FirstOrDefault(x => x.Id == course.Id);
                if (image != null)
                {
                    var name = Path.Combine(_he.WebRootPath + "/Images", Path.GetFileName(image.FileName));
                    await image.CopyToAsync(new FileStream(name, FileMode.Create));
                    //course.ImageUrl = "Images/" + image.FileName;
                    prevCourse.ImageUrl = "Images/" + image.FileName;
                }
                if (image == null && prevCourse.ImageUrl == null)
                {
                    prevCourse.ImageUrl = "Images/noimage.png";
                }

                prevCourse.Title = course.Title;
                prevCourse.Description = course.Description;
                prevCourse.Duration = course.Duration;
                prevCourse.Price = course.Price;
                prevCourse.IntroVideoUrl = course.IntroVideoUrl;
                await _db.SaveChangesAsync();
                TempData["update"] = "Course Updated Successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(course);
        }
        [HttpGet]

        public ActionResult Details(int? id)
        {
            ViewBag.Roles = HttpContext.Session.GetString("UserRoles");
            if (id == null)
            {
                return NotFound();
            }
            var course = _db.Course.FirstOrDefault(c => c.Id == id);
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }

        //Get Delete Action Method
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var course = _db.Course.FirstOrDefault(c => c.Id == id);
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }
        //Post Delete Action Method
        [HttpPost]
        [ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirm(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var course = _db.Course.FirstOrDefault(c => c.Id == id);
            if (course == null)
            {
                return NotFound();
            }
            var modules = _db.Module.Where(m => m.CourseId == id).ToList();
            if (modules.Any())
            {
                _db.Module.RemoveRange(modules);
            }
            _db.Course.Remove(course);
            await _db.SaveChangesAsync();
            TempData["delete"] = "Course Deleted Successfully";
            return RedirectToAction(nameof(Index));
        }
    }
}
