using LMS.Data;
using LMS.Models.DBModels;
using LMS.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Areas.Admin
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ModuleController : Controller
    {
        private ApplicationDbContext _db;
        public ModuleController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            return View(_db.Course.ToList());
        }
        [HttpGet]
        public IActionResult Create(int id)
        {
            var course = _db.Course.FirstOrDefault(c => c.Id == id);
            ViewBag.Course = course!.Title;
            ViewBag.CourseId = course.Id;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Module module)
        {
            if (ModelState.IsValid)
            {
                var searchProduct = _db.Module.FirstOrDefault(c => c.Title == module.Title && c.CourseId == module.CourseId);

                if (searchProduct != null)
                {
                    ViewBag.message = "This Module is already exist";
                    return View(module);
                }
                module.Id = 0;
                _db.Module.Add(module);
                await _db.SaveChangesAsync();
                TempData["save"] = "Module Saved Successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(module);
        }
        [HttpGet]
        public IActionResult Details(int id)
        {
            var course = _db.Course.FirstOrDefault(c => c.Id == id);
            ViewBag.Course = course!.Title;
            var data = _db.Module.Where(x => x.CourseId == id).OrderBy(x => x.ModuleOrder).ToList();
            return View(data);
        }
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var module = _db.Module.FirstOrDefault(x => x.Id == id);
            if (module != null)
            {
                var course = _db.Course.FirstOrDefault(x => x.Id == module.CourseId);
                ViewBag.Course = course!.Title;
                ViewBag.CourseId = course.Id;
            }
            return View(module);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(Module module)
        {
            if (ModelState.IsValid)
            {
                _db.Module.Update(module);
                await _db.SaveChangesAsync();
                TempData["update"] = "Module Updated Successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(module);
        }
        public IActionResult AddModuleQuiz(int id)
        {
            var module = _db.Module.Include(m => m.Course).FirstOrDefault(m => m.Id == id);
            ViewBag.Module = module!.Title;
            ViewBag.ModuleId = module.Id;
            ViewBag.Course = module.Course!.Title;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddModuleQuiz(QuizVm quizVm)
        {
            if (ModelState.IsValid)
            {
                var searchProduct = _db.QuizQuestion.FirstOrDefault(c => c.Question == quizVm.Question);
                if (searchProduct != null)
                {
                    ViewBag.message = "This Question of this module is already exist";
                    return View(quizVm);
                }

                var quiz = _db.Quiz.FirstOrDefault(q => q.ModuleId == quizVm.ModuleId);
                if (quiz == null)
                {
                    quiz = new Quiz
                    {
                        ModuleId = quizVm.ModuleId,
                        Title = quizVm.ModuleTitle! + " Quiz Test"
                    };
                    _db.Quiz.Add(quiz);
                    await _db.SaveChangesAsync(); // Save to get quiz.Id
                }

                var quizQuestion = new QuizQuestion
                {
                    QuizId = quiz.Id,
                    Question = quizVm.Question!,
                    OptionA = quizVm.OptionA!,
                    OptionB = quizVm.OptionB!,
                    OptionC = quizVm.OptionC!,
                    OptionD = quizVm.OptionD!,
                    CorrectOption = quizVm.CorrectOption!
                };
                _db.QuizQuestion.Add(quizQuestion);
                await _db.SaveChangesAsync();
                TempData["save"] = "Quiz Question Saved Successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(quizVm);
        }
        public IActionResult QuizQuestion(int id)
        {
            var quiz = _db.Quiz.Include(q => q.Module).ThenInclude(m => m.Course).FirstOrDefault(q => q.ModuleId == id);
            if (quiz == null) return NotFound();
            var questions = _db.QuizQuestion.Where(q => q.QuizId == quiz.Id).ToList();
            var model = new QuizVm
            {
                CourseTitle = quiz.Module.Course.Title,
                ModuleTitle = quiz.Module.Title,
                ModuleId = quiz.Module.Id,
                QuizId = quiz.Id,
                Title = quiz.Title,
                QuizQuestion = questions
            };
            return View(model);
        }
        [HttpGet]
        public IActionResult EditQuizQuestion(int id)
        {
            var quizQuestion = _db.QuizQuestion.FirstOrDefault(q => q.Id == id);
            var quiz = _db.Quiz.Include(m => m.Module).ThenInclude(c => c.Course).FirstOrDefault(q => q.Id == quizQuestion!.QuizId);
            if (quiz == null) return NotFound();
            var model = new QuizVm
            {
                ModuleTitle = quiz.Module.Title,
                ModuleId = quiz.Module!.Id,
                CourseTitle = quiz.Module.Course!.Title,
                Title = quiz.Title,
                Id = quizQuestion.Id,
                QuizId = quiz.Id,
                Question = quizQuestion.Question,
                OptionA = quizQuestion.OptionA,
                OptionB = quizQuestion.OptionB,
                OptionC = quizQuestion.OptionC,
                OptionD = quizQuestion.OptionD,
                CorrectOption = quizQuestion.CorrectOption,
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> EditQuizQuestion(QuizVm quizVm)
        {
            if (ModelState.IsValid)
            {
                var quizQuestion = new QuizQuestion
                {
                    Id = quizVm.Id,
                    QuizId = quizVm.QuizId,
                    Question = quizVm.Question!,
                    OptionA = quizVm.OptionA!,
                    OptionB = quizVm.OptionB!,
                    OptionC = quizVm.OptionC!,
                    OptionD = quizVm.OptionD!,
                    CorrectOption = quizVm.CorrectOption!
                };
                _db.QuizQuestion.Update(quizQuestion);
                await _db.SaveChangesAsync();
                TempData["update"] = "Quiz Question Updated Successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(quizVm);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteQuizQuestion(int id)
        {
            var question = await _db.QuizQuestion.FindAsync(id);
            if (question == null) return NotFound();
            _db.QuizQuestion.Remove(question);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
