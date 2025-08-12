using LMS.Data;
using LMS.Models.DBModels;
using LMS.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Security.Claims;

namespace LMS.Areas.Student
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class CourseController : Controller
    {
        private ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public CourseController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }
        [HttpGet]
        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var enrollments = _db.Enrollments
            .Where(e => e.StudentId == userId && e.IsCompleted == true)
            .Include(e => e.Course)
            .Select(e => new EnrollmentVm
            {
                CourseName = e.Course!.Title,
                CourseId = e.Course.Id,
                CoursePrice = e.Course.Price,
                EnrollmentDate = e.EnrollmentDate,
                ProgressTrackings = _db.ProgressTracking
                    .Where(p => p.StudentId == userId && p.CourseId == e.CourseId)
                    .ToList(),
                ProgressPercent = _db.Module
                    .Where(m => m.CourseId == e.CourseId)
                    .Count() == 0
                    ? 0
                    : (double)_db.ProgressTracking
                        .Count(p => p.StudentId == userId && p.CourseId == e.CourseId && p.Completed)
                        / _db.Module.Count(m => m.CourseId == e.CourseId)
                        * 100,

                IsFinalExamPassed = _db.FinalExam.Any(f => f.CourseId == e.CourseId) &&
                    _db.FinalExam
                        .Where(f => f.CourseId == e.CourseId)
                        .All(f => _db.FinalExamSubmission
                            .Any(s => s.StudentId == userId && s.FinalExamId == f.Id && s.Passed))


            })
                    .ToList();


            return View(enrollments);
        }
        [HttpGet]
        public async Task<IActionResult> Enroll(int id) //course id
        {
            var course = await _db.Course.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }
        [HttpPost]
        public async Task<IActionResult> EnrollConfirmed(int id)
        {
            var course = await _db.Course.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            var existingEnrollment = await _db.Enrollments
                .FirstOrDefaultAsync(e => e.CourseId == id && e.StudentId == user.Id && e.IsCompleted == true);

            if (existingEnrollment != null)
            {
                ViewBag.message = "You are already enrolled in this course.";
                return RedirectToAction("Index");
            }

            var enrollment = new Enrollments
            {
                StudentId = user!.Id,
                CourseId = id,
                EnrollmentDate = DateTime.Now,
                IsCompleted = course.IsPaid ? false : true,
            };
            _db.Enrollments.Add(enrollment);
            _db.SaveChanges();
            // If course is paid, create a Payment record
            if (course.IsPaid)
            {
                var payment = new Payment
                {
                    StudentId = user.Id,
                    EnrollmentsId = enrollment.Id,
                    Amount = course.Price,
                    PaymentStatus = false,
                    PaymentDate = DateTime.Now
                };
                _db.Payment.Add(payment);
                _db.SaveChanges();

                // Redirect to payment page
                return RedirectToAction("Payment", new { id = enrollment.Id });
            }

            TempData["save"] = "You have successfully enrolled!";
            return RedirectToAction("Index");
        }
        [HttpGet]
        public IActionResult Payment(int id) // enrollment Id
        {
            var payment = _db.Payment.Include(x => x.Enrollments).ThenInclude(x => x.Course).FirstOrDefault(x => x.EnrollmentsId == id);
            return View(payment);
        }
        [HttpPost]
        public IActionResult ConfirmPayment(int id) //payment Id
        {
            var payment = _db.Payment
                .Include(p => p.Enrollments)
                .FirstOrDefault(p => p.Id == id);

            if (payment == null)
            {
                return NotFound();
            }

            payment.PaymentStatus = true;
            payment.PaymentDate = DateTime.Now;
            payment.Enrollments!.IsCompleted = true;
            _db.SaveChanges();
            // Redirect to lessons page
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Lessons(int id) // courseId
        {
            ViewBag.Message = TempData["message"];
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var modules = _db.Module
                .Where(m => m.CourseId == id)
                .OrderBy(m => m.ModuleOrder)
                .ToList();

            var completedModuleIds = _db.ProgressTracking
                .Where(p => p.StudentId == userId && p.CourseId == id)
                .Select(p => p.ModuleId)
                .ToList();

            var completedQuizModuleIds = _db.QuizResult
                .Where(qr => qr.StudentId == userId && qr.Passed)
                .Select(qr => qr.Quiz.ModuleId)
                .ToList();

            var moduleVms = new List<ModuleVm>();

            bool previousModuleCompleted = true; // first module always unlocked
            bool previousQuizPassed = true;      // first module always unlocked

            foreach (var module in modules)
            {
                bool isCompleted = completedModuleIds.Contains(module.Id);
                bool quizPassed = completedQuizModuleIds.Contains(module.Id);

                bool isUnlocked = false;

                if (module.ModuleOrder == 1)
                {
                    isUnlocked = true; // first module always unlocked
                }
                else
                {
                    isUnlocked = previousModuleCompleted && previousQuizPassed;
                }

                moduleVms.Add(new ModuleVm
                {
                    Id = module.Id,
                    Title = module.Title,
                    ModuleOrder = module.ModuleOrder,
                    Contents = module.Contents,
                    IsCompleted = isCompleted,
                    IsUnlocked = isUnlocked,
                    IsPassed = quizPassed
                });

                // Update for next iteration
                previousModuleCompleted = isCompleted;
                previousQuizPassed = quizPassed;
            }

            return View(moduleVms);
        }

        [HttpPost]

        public async Task<IActionResult> CompleteModule(int id) // module id
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var module = await _db.Module.FindAsync(id);
            if (module == null)
            {
                TempData["error"] = "Module not found.";
                return RedirectToAction("Index");
            }

            // Check if progress already exists
            var existingProgress = await _db.ProgressTracking
                .FirstOrDefaultAsync(p => p.StudentId == userId && p.ModuleId == id);

            if (existingProgress != null)
            {
                // Update existing record
                existingProgress.Completed = true;
                existingProgress.CompletionDate = DateTime.UtcNow;
            }
            else
            {
                // Create new record
                var progress = new ProgressTracking
                {
                    StudentId = userId,
                    CourseId = module.CourseId,
                    ModuleId = id,
                    Completed = true, // since you're marking it completed
                    CompletionDate = DateTime.UtcNow
                };
                _db.ProgressTracking.Add(progress);
            }

            await _db.SaveChangesAsync();

            TempData["save"] = "Module marked as completed!";
            return RedirectToAction("Lessons", new { id = module.CourseId });
        }
        //public async Task<IActionResult> CompleteModule(int id) //module id
        //{
        //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        //    var existingProgress = await _db.ProgressTracking.FirstOrDefaultAsync(p => p.StudentId == userId && p.ModuleId == id);

        //    var module = await _db.Module.FindAsync(id);
        //    var progress = new ProgressTracking
        //    {
        //        StudentId = userId,
        //        CourseId = module.CourseId,
        //        ModuleId = id,
        //        Completed = false,
        //        CompletionDate = DateTime.UtcNow
        //    };
        //    _db.ProgressTracking.Add(progress);
        //    await _db.SaveChangesAsync();
        //    TempData["save"] = "Module marked as completed!";
        //    return RedirectToAction("Lessons", new { id = module.CourseId });
        //}
        [HttpGet]
        public IActionResult TakeQuiz(int id) // module id
        {
            var quiz = _db.Quiz
            .Include(q => q.QuizQuestions)
            .FirstOrDefault(q => q.ModuleId == id);
            var courseId = _db.Module.Find(id)?.CourseId;
            ViewBag.CourseId = courseId;
            if (quiz == null)
            {
                TempData["message"] = "No quiz found for this module.";
                return RedirectToAction("Lessons", new { id = courseId });
            }

            return View(quiz);

        }

        [HttpPost]
        public IActionResult TakeQuiz(int quizId, Dictionary<int, string> answers)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var quiz = _db.Quiz
                .Include(q => q.QuizQuestions)
                .FirstOrDefault(q => q.Id == quizId);
            var courseId = _db.Module.Find(quiz.ModuleId).CourseId;
            var progressTracking = _db.ProgressTracking.FirstOrDefault(x => x.ModuleId == quiz.ModuleId && x.StudentId == userId);
            if (quiz == null)
            {
                TempData["message"] = "Quiz not found.";
                return RedirectToAction("Lessons", new { courseId = _db.Quiz.Find(quizId)?.Module.CourseId });
            }

            int totalQuestions = quiz.QuizQuestions.Count;
            int correctAnswers = 0;
            var studentAnswers = new List<StudentAnswerVm>();

            // Create QuizResult record (but don't save yet — we need the ID first)
            var quizResult = new QuizResult
            {
                StudentId = userId,
                QuizId = quiz.Id,
                AttemptDate = DateTime.UtcNow,
                Score = 0, // will update later
                Passed = false // will update later
            };
            _db.QuizResult.Add(quizResult);
            _db.SaveChanges(); // save to get the QuizResult ID

            // For each question, record the student's answer
            foreach (var question in quiz.QuizQuestions)
            {
                string studentAnswer = answers.ContainsKey(question.Id) ? answers[question.Id] : "";
                bool isCorrect = string.Equals(studentAnswer, question.CorrectOption, StringComparison.OrdinalIgnoreCase);

                if (isCorrect)
                {
                    correctAnswers++;
                }

                // Save each student's answer
                var studentQuizAnswer = new StudentQuizAnswer
                {
                    QuizResultId = quizResult.Id,
                    QuizQuestionId = question.Id,
                    SelectedOption = studentAnswer,
                    IsCorrect = isCorrect
                };
                _db.StudentQuizAnswer.Add(studentQuizAnswer);

                // For showing in the view (optional)
                studentAnswers.Add(new StudentAnswerVm
                {
                    Question = question.Question,
                    OptionA = question.OptionA,
                    OptionB = question.OptionB,
                    OptionC = question.OptionC,
                    OptionD = question.OptionD,
                    StudentAnswer = studentAnswer,
                    CorrectAnswer = question.CorrectOption,
                    IsCorrect = isCorrect
                });
            }

            // Update QuizResult final values
            quizResult.Score = correctAnswers;
            //quizResult.TotalQuestions = totalQuestions;
            quizResult.Passed = correctAnswers >= (int)Math.Ceiling(totalQuestions * 0.5); // e.g., 50% pass
            if (quizResult.Passed)
            {
                progressTracking.Completed = true;
            }

            _db.SaveChanges();

            // 4️⃣ Show result to the student
            var resultVm = new QuizResultVm
            {
                CourseId = courseId,
                ModuleId = quiz.ModuleId,
                Score = correctAnswers,
                TotalQuestions = totalQuestions,
                Passed = quizResult.Passed,
                StudentAnswers = studentAnswers
            };

            return View("QuizResult", resultVm);
        }

        public IActionResult QuizResults(int id) //Module Id
        {
            var courseId = _db.Module.Find(id)?.CourseId;
            ViewBag.CourseId = courseId;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var quizResult = _db.QuizResult
                .Include(qr => qr.Quiz)
                    .ThenInclude(q => q.QuizQuestions)
                .Where(qr => qr.Quiz.ModuleId == id && qr.StudentId == userId)
                .OrderByDescending(qr => qr.AttemptDate)
                .FirstOrDefault();


            if (quizResult == null)
            {
                TempData["error"] = "Quiz result not found.";
                return RedirectToAction("Lessons", new { courseId = quizResult?.Quiz.Module.CourseId });
            }

            var studentAnswers = _db.StudentQuizAnswer
                .Where(a => a.QuizResultId == quizResult.Id)
                .Include(a => a.QuizQuestion)
                .Select(a => new StudentAnswerVm
                {
                    Question = a.QuizQuestion.Question,
                    OptionA = a.QuizQuestion.OptionA,
                    OptionB = a.QuizQuestion.OptionB,
                    OptionC = a.QuizQuestion.OptionC,
                    OptionD = a.QuizQuestion.OptionD,
                    StudentAnswer = a.SelectedOption,
                    CorrectAnswer = a.QuizQuestion.CorrectOption,
                    IsCorrect = a.IsCorrect
                })
                .ToList();

            var resultVm = new QuizResultVm
            {
                Score = quizResult.Score,
                TotalQuestions = quizResult.Quiz.QuizQuestions.Count,
                Passed = quizResult.Passed,
                StudentAnswers = studentAnswers
            };

            return View(resultVm);
        }
        [HttpGet]
        public IActionResult FinalExam(int id) // Course id
        {
            ViewBag.Message = TempData["message"];
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var exams = _db.FinalExam
                .Where(x => x.CourseId == id)
                .Include(e => e.Course)
                .Include(e => e.FinalExamSubmission)
                .ToList();

            var examVms = exams.Select(e => new FinalExamStatusVm
            {
                Exam = e,
                UserId = userId,
                HasPassed = e.FinalExamSubmission!
                    .Any(s => s.StudentId == userId && s.Passed)
            }).ToList();

            return View(examVms);
        }

        [HttpGet]
        public IActionResult TakeExam(int id) // exam id
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var exam = _db.FinalExam.Include(m => m.Course).FirstOrDefault(e => e.Id == id);
            if (exam == null)
            {
                TempData["error"] = "No exam found for this course.";
                return RedirectToAction("TakeExam", new { id = id });
            }

            ViewBag.CourseTitle = exam.Course!.Title;
            ViewBag.CourseId = exam.CourseId;
            ViewBag.ExamTitle = exam.Title;
            ViewBag.ExamId = exam.Id;
            ViewBag.ExamType = exam.ExamType;

            // Count previous attempts
            var previousAttempts = _db.FinalExamSubmission
                .Count(s => s.FinalExamId == exam.Id && s.StudentId == userId);

            if (previousAttempts >= exam.MaxAttempts)
            {
                TempData["message"] = "You have reached the maximum number of attempts.";
                return RedirectToAction("FinalExam", new { id = exam.CourseId });
            }

            // Create new submission attempt
            var submission = new FinalExamSubmission
            {
                FinalExamId = exam.Id,
                StudentId = userId,
                Attempts = previousAttempts + 1,
                SubmissionDate = DateTime.UtcNow,
                Passed = false,
                Score = 0
            };
            _db.FinalExamSubmission.Add(submission);
            _db.SaveChanges();

            var usedAttempts = previousAttempts + 1;
            ViewBag.UsedAttempts = usedAttempts;
            ViewBag.MaxAttempts = exam.MaxAttempts;
            ViewBag.AttemptsLeft = exam.MaxAttempts - usedAttempts;
            ViewBag.SubmissionId = submission.Id;
            if (exam.ExamType == "MCQ") // MCQ
            {
                var questions = _db.FinalExamQuestion
                    .Where(q => q.FinalExamId == exam.Id)
                    .ToList();

                return View("TakeExamMCQ", questions);
            }
            else // Written
            {
                var questions = _db.FinalExamQuestion
                    .Where(q => q.FinalExamId == exam.Id)
                    .ToList();

                ViewBag.Questions = questions;
                return View("TakeExamWritten", exam);
            }
        }
        [HttpPost]
        public async Task<IActionResult> SubmitWrittenExam(int ExamId, int SubmissionId, IFormFile AnswerFile)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (AnswerFile == null || AnswerFile.Length == 0)
            {
                TempData["error"] = "Please upload a PDF file.";
                return RedirectToAction("TakeExam", new { courseId = _db.FinalExam.Find(ExamId)?.CourseId });
            }

            // Save file
            var fileName = Guid.NewGuid() + Path.GetExtension(AnswerFile.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Uploads", fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await AnswerFile.CopyToAsync(stream);
            }

            var studentAnswer = new StudentFinalExamAnswer
            {
                FinalExamSubmissionId = SubmissionId,
                WrittenSubmissionURL = "/Uploads/" + fileName
            };
            _db.StudentFinalExamAnswer.Add(studentAnswer);
            await _db.SaveChangesAsync();

            TempData["save"] = "Your written exam has been submitted successfully.";
            return RedirectToAction("FinalExam", new { id = _db.FinalExam.Find(ExamId)!.CourseId });
        }

        [HttpPost]
        public IActionResult SubmitMCQExam(FinalExamAnswerVm model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var exam = _db.FinalExam.FirstOrDefault(e => e.Id == model.ExamId);
            var questions = _db.FinalExamQuestion.Where(q => q.FinalExamId == model.ExamId).ToList();

            int score = 0;
            foreach (var question in questions)
            {
                string selected = model.Answers.TryGetValue(question.Id, out var val) ? val : "";
                bool isCorrect = selected == question.CorrectOption;
                if (isCorrect) score++;

                _db.StudentFinalExamAnswer.Add(new StudentFinalExamAnswer
                {
                    FinalExamSubmissionId = model.SubmissionId,
                    FinalExamQuestionId = question.Id,
                    SelectedOption = selected,
                    IsCorrect = isCorrect
                });
            }

            // Update the existing submission
            var submission = _db.FinalExamSubmission.Find(model.SubmissionId);
            int total = questions.Count;
            int percentage = (int)((score * 100.0) / total);
            submission.Score = score;
            submission.Passed = percentage >= exam.PassingScore;
            _db.SaveChanges();

            return RedirectToAction("ExamResult", new { id = submission.Id });
        }

        [HttpGet]
        public IActionResult ExamResult(int id) // Final Exam Submission id
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var submission = _db.FinalExamSubmission
                .Include(s => s.FinalExam)
                .FirstOrDefault(s => s.Id == id && s.StudentId == userId);

            if (submission == null)
            {
                TempData["error"] = "Exam result not found.";
                return RedirectToAction("Index", "Student");
            }

            var examType = submission.FinalExam.ExamType == "Written" ? "Written" : "MCQ";
            var vm = new FinalExamResultVm
            {
                CourseId = submission.FinalExam.CourseId,
                ExamTitle = submission.FinalExam.Title,
                ExamType = examType,
                Score = submission.Score,
                Passed = submission.Passed,
                SubmissionDate = submission.SubmissionDate
            };

            if (submission.FinalExam.ExamType == "Written") // Written
            {
                var writtenAnswer = _db.StudentFinalExamAnswer
                    .FirstOrDefault(a => a.FinalExamSubmissionId == id);

                vm.WrittenSubmissionUrl = writtenAnswer?.WrittenSubmissionURL;
            }
            else // MCQ
            {
                var answers = _db.StudentFinalExamAnswer
                    .Where(a => a.FinalExamSubmissionId == id && a.FinalExamQuestionId != null)
                    .Include(a => a.FinalExamQuestion)
                    .ToList();

                vm.TotalQuestions = answers.Count;

                vm.Answers = answers.Select(a => new StudentAnswerVm
                {
                    Question = a.FinalExamQuestion.Question,
                    OptionA = a.FinalExamQuestion.OptionA,
                    OptionB = a.FinalExamQuestion.OptionB,
                    OptionC = a.FinalExamQuestion.OptionC,
                    OptionD = a.FinalExamQuestion.OptionD,
                    StudentAnswer = a.SelectedOption,
                    CorrectAnswer = a.FinalExamQuestion.CorrectOption,
                    IsCorrect = a.IsCorrect
                }).ToList();
            }

            return View(vm);
        }

        [HttpGet]

        public IActionResult ViewResult(int id) //Final Exam Id
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var exam = _db.FinalExam.FirstOrDefault(e => e.Id == id);
            if (exam == null)
                return NotFound();

            var submission = _db.FinalExamSubmission
                .FirstOrDefault(s => s.FinalExamId == id && s.StudentId == userId);

            if (submission == null)
            {
                TempData["message"] = "No submission found.";
                return RedirectToAction("FinalExam", new { id = exam.CourseId });
            }

            var model = new FinalExamResultVm
            {
                Exam = exam,
                Submission = submission
            };

            var questions = _db.FinalExamQuestion
                .Where(q => q.FinalExamId == exam.Id)
                .ToList();

            var answers = _db.StudentFinalExamAnswer
            .Where(a => a.FinalExamSubmissionId == submission.Id)
            .ToList();
            if (exam.ExamType == "MCQ") // 
            {
                var result = questions.Select(q => new
                {
                    Question = q,
                    Answer = answers.FirstOrDefault(a => a.FinalExamQuestionId == q.Id)
                });

                model.StudentFinalExamAnswer = questions.Select(q =>
                {
                    // Find existing answer for this question
                    var existingAnswer = answers.FirstOrDefault(a => a.FinalExamQuestionId == q.Id);

                    // If answer exists, return it
                    if (existingAnswer != null)
                    {
                        return existingAnswer;
                    }

                    // If no answer exists, create a new empty answer record
                    return new StudentFinalExamAnswer
                    {
                        FinalExamQuestionId = q.Id,
                        FinalExamQuestion = q,
                        FinalExamSubmissionId = submission.Id,
                        SelectedOption = null, // or string.Empty
                        IsCorrect = false,
                        WrittenSubmissionURL = null
                    };
                }).ToList();
            }
            else if (exam.ExamType == "Written")
            {
                var writtenAnswer = _db.StudentFinalExamAnswer
                .Where(a => a.FinalExamSubmissionId == submission.Id)
                .FirstOrDefault();
                // For written exams, just assign the existing answers (should be 1 ideally)
                model.StudentFinalExamAnswer = questions.Select(q => new StudentFinalExamAnswer
                {
                    FinalExamQuestionId = q.Id,
                    FinalExamQuestion = q,
                    FinalExamSubmissionId = submission.Id,
                    WrittenSubmissionURL = writtenAnswer.WrittenSubmissionURL
                }).ToList();
                ViewBag.TotalMarks = questions.Sum(q => q.Marks ?? 0);
            }

            return View("ViewResult", model);
        }
        [HttpGet]
        public IActionResult Certificate(int id) // course Id
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var course = _db.Course.FirstOrDefault(c => c.Id == id);
            var user = _db.Users.FirstOrDefault(u => u.Id == userId);
            var submissionDate = _db.FinalExamSubmission
                .Where(s => s.StudentId == userId && s.FinalExam.CourseId == id && s.Passed)
                .OrderByDescending(s => s.SubmissionDate)
                .Select(s => s.SubmissionDate)
                .FirstOrDefault();

            if (course == null || user == null || submissionDate == default)
            {
                TempData["message"] = "Certificate not available yet.";
                return RedirectToAction("Index");
            }

            var model = new CertificateVm
            {
                //StudentName = user.FullName ?? user.UserName, // Customize field as per your model
                StudentName = user.FullName!, // Customize field as per your model
                CourseTitle = course.Title,
                CompletionDate = submissionDate
            };

            return View(model);
        }

    }


}

