using LMS.Data;
using LMS.Models.DBModels;
using LMS.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LMS.Areas.Admin
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class FinalExamController : Controller
    {
        private ApplicationDbContext _db;
        public FinalExamController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            var exams = _db.FinalExam.Include(e => e.Course).Include(e => e.FinalExamSubmission).ToList();
            return View(exams);
        }
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Courses = _db.Course.ToList();
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(FinalExam model)
        {
            if (ModelState.IsValid)
            {
                var searchExam = _db.FinalExam.FirstOrDefault(c => c.Title == model.Title && c.CourseId == model.CourseId);
                if (searchExam != null)
                {
                    ViewBag.Courses = _db.Course.ToList();
                    ViewBag.message = "This Final exam is already exist";
                    return View(model);
                }

                _db.FinalExam.Add(model);
                await _db.SaveChangesAsync();
                TempData["save"] = "Final exam created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }
        [HttpGet]
        public IActionResult Edit(int id)
        {
            ViewBag.Courses = _db.Course.ToList();
            var exam = _db.FinalExam.FirstOrDefault(x => x.Id == id);
            return View(exam);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(FinalExam model)
        {
            if (ModelState.IsValid)
            {
                _db.FinalExam.Update(model);
                await _db.SaveChangesAsync();
                TempData["update"] = "Final Exam Updated Successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var exam = await _db.FinalExam.FindAsync(id);
            if (exam == null) return NotFound();
            _db.FinalExam.Remove(exam);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        public IActionResult Question(int id)
        {
            var exams = _db.FinalExam.Include(e => e.Course).FirstOrDefault(q => q.Id == id);
            if (exams == null) return NotFound();
            var question = _db.FinalExamQuestion.Where(x => x.FinalExamId == exams.Id).ToList();
            var model = new FinalExamVm
            {
                CourseTitle = exams.Course!.Title,
                CourseId = exams.Course!.Id,
                ExamId = exams.Id,
                ExamType = exams.ExamType,
                Title = exams.Title,
                FinalExamQuestion = question,
            };
            return View(model);
        }

        [HttpGet]
        public IActionResult CreateQuestion(int id)
        {
            var exam = _db.FinalExam.Include(c => c.Course).FirstOrDefault(c => c.Id == id);
            ViewBag.Exam = exam!.Title;
            ViewBag.ExamType = exam.ExamType;
            ViewBag.ExamId = exam.Id;
            ViewBag.Course = exam.Course!.Title;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateQuestion(FinalExamVm model)
        {
            if (ModelState.IsValid)
            {
                var searchProduct = _db.FinalExamQuestion.FirstOrDefault(c => c.Question == model.Question);
                if (searchProduct != null)
                {
                    ViewBag.message = "This Question of this exam is already exist";
                    return View(model);
                }
                var question = new FinalExamQuestion
                {
                    FinalExamId = model.ExamId,
                    Question = model.Question!,
                    OptionA = model.OptionA!,
                    OptionB = model.OptionB!,
                    OptionC = model.OptionC!,
                    OptionD = model.OptionD!,
                    CorrectOption = model.CorrectOption!,
                    Marks = model.Marks!,
                };
                _db.FinalExamQuestion.Add(question);
                await _db.SaveChangesAsync();
                TempData["save"] = "Final Exam Question Saved Successfully";
                return RedirectToAction("Question", new { id = model.ExamId });
            }
            return View(model);
        }
        [HttpGet]
        public IActionResult EditQuestion(int id)
        {
            var question = _db.FinalExamQuestion.FirstOrDefault(x => x.Id == id);
            var exams = _db.FinalExam.Include(m => m.Course).FirstOrDefault(x => x.Id == question.FinalExamId);

            var model = new FinalExamVm
            {
                Id = question!.Id,
                CourseTitle = exams!.Course!.Title,
                CourseId = exams.Course!.Id,
                ExamId = exams.Id,
                ExamType = exams.ExamType,
                Title = exams.Title,
                Question = question!.Question,
                OptionA = question.OptionA,
                OptionB = question.OptionB,
                OptionC = question.OptionC,
                OptionD = question.OptionD,
                CorrectOption = question.CorrectOption,
                Marks = question.Marks
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> EditQuestion(FinalExamVm model)
        {
            if (ModelState.IsValid)
            {
                var question = new FinalExamQuestion
                {
                    Id = model.Id,
                    FinalExamId = model.ExamId,
                    Question = model.Question!,
                    OptionA = model.OptionA!,
                    OptionB = model.OptionB!,
                    OptionC = model.OptionC!,
                    OptionD = model.OptionD!,
                    CorrectOption = model.CorrectOption!,
                    Marks = model.Marks
                };
                _db.FinalExamQuestion.Update(question);
                await _db.SaveChangesAsync();
                TempData["update"] = "Final Exam Updated Successfully";
                return RedirectToAction("Question", new { id = model.ExamId });
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var question = await _db.FinalExamQuestion.FindAsync(id);
            if (question == null) return NotFound();
            _db.FinalExamQuestion.Remove(question);
            await _db.SaveChangesAsync();

            return RedirectToAction("Question", new { id = question.FinalExamId });
        }
        [HttpGet]
        public IActionResult Mark(int id) // FinalExamId
        {
            var submissions = _db.FinalExamSubmission
                .Include(s => s.FinalExam)
                .Include(s => s.Student)
                .Where(x => x.FinalExamId == id)
                .ToList();

            if (!submissions.Any())
                return NotFound();

            ViewBag.TotalMarks = _db.FinalExamQuestion
                .Where(x => x.FinalExamId == id)
                .Sum(x => x.Marks ?? 0);

            // Get written answers for each submission
            var writtenAnswers = _db.StudentFinalExamAnswer
                .Where(a => a.WrittenSubmissionURL != null)
                .ToList();

            ViewBag.WrittenAnswers = writtenAnswers;

            return View(submissions);
        }


        [HttpPost]
        public async Task<IActionResult> Mark(int id, int score, int totalMarks)
        {
            var submission = _db.FinalExamSubmission.Include(x => x.FinalExam).FirstOrDefault(x => x.Id == id);
            if (submission == null)
                return NotFound();
            //var totalMarks = _db.FinalExamQuestion
            //.Where(x => x.FinalExamId == submission.FinalExamId)
            //.Sum(x => x.Marks ?? 0);

            submission.Score = score;
            int percentage = (int)((score * 100.0) / totalMarks);
            submission.Passed = percentage >= submission.FinalExam.PassingScore; // Optional logic
            await _db.SaveChangesAsync();

            TempData["save"] = "Score submitted successfully.";
            return RedirectToAction("Index");
        }

    }
}
