# 📚 Learning Management System (LMS)

A role-based Learning Management System (LMS) built with **ASP.NET Core 8.0 MVC** and **Entity Framework Core (Code-First)**.  
Supports **Admin**, **Instructor**, and **Student** roles with modular learning, assessments (MCQ + written), progress tracking, and certificate generation.

**Repo:** https://github.com/Masavi99/LMS

---

## ✨ Key Features

- **Authentication & Authorization**
  - ASP.NET Identity with role-based access (**Admin / Instructor / Student**)

- **Role-based Dashboards**
  - Different views and actions per role (management vs learning)

- **Course → Module → Assessment Flow**
  - Courses contain modules
  - **Sequential unlocking**: students must pass the module quiz to unlock the next module

- **Quizzes (MCQ)**
  - Auto-graded
  - Configurable passing score and attempt limits
  - Instant pass/fail feedback

- **Final Exam (Hybrid)**
  - **MCQ**: auto-graded
  - **Written**: student uploads **PDF**, graded manually by instructor/admin

- **Progress Tracking**
  - Tracks course/module completion and performance
  - Student progress view/dashboard

- **Certificate Generation**
  - Auto-generated **PDF certificate** after meeting completion rules (quizzes + final exam)

- **Payments (Planned)**
  - Paid enrollments / payment gateway integration (future enhancement)

---

## 🧱 Tech Stack

- **Backend:** ASP.NET Core 8.0 MVC (C#)
- **Database:** Microsoft SQL Server 2019+
- **ORM:** Entity Framework Core (Code-First, Migrations)
- **Auth:** ASP.NET Core Identity (roles + policies)
- **Frontend:** Razor Views, Bootstrap 5, jQuery
- **Tooling:** .NET 8 SDK, Visual Studio 2022, Git/GitHub

---

## ⚙️ Local Setup (Windows / macOS)

### 1) Clone the repository
```bash
git clone https://github.com/Masavi99/LMS.git
cd LMS
2) Configure the database connection string
Create/update appsettings.json (recommended: appsettings.Development.json):
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SQL_SERVER;Database=LMS;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}

3) Apply EF Core migrations
dotnet tool restore
dotnet ef database update
4) Run the project
dotnet run

🧪 Migrations (if starting fresh)
If you need to create migrations from scratch:
dotnet ef migrations add InitialCreate
dotnet ef database update

👥 Roles (Quick Overview)
•	Admin: manage users/roles, courses, exams, reporting
•	Instructor: create modules/quizzes, mark written exams
•	Student: enroll, learn modules, attempt quizzes/exams, download certificates

📌 Notes
•	Written exam submissions require file upload support (PDF).
•	Certificates are generated only when all completion rules are satisfied.
•	For production deployment, use secure secrets management and consider external storage for uploaded files.

