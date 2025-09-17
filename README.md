# 📚 Learning Management System (LMS)

Role-based LMS built with **ASP.NET Core 8.0 MVC** and **Entity Framework Core**.  
Supports **Admins**, **Instructors**, and **Students** with courses, modules, quizzes, final exams (MCQ + written), progress tracking, certificates, and payments.

> Repo: https://github.com/Masavi99/LMS

---

## ✨ Features

- **Auth & Roles**: ASP.NET Identity (Admin / Instructor / Student)
- **Dashboards**: role-specific views & actions
- **Courses & Modules**: sequential unlocking based on quiz pass
- **Quizzes**: MCQ, auto-graded, attempt limits
- **Final Exams**: MCQ + PDF written upload, manual grading
- **Progress Tracking**: per module; completion + timestamp
- **Certificates**: auto PDF on course completion
- **Payments**: gateway integration for paid enrollments

---

## 🧱 Tech Stack

- **Backend**: ASP.NET Core 8.0 MVC, C#
- **DB**: SQL Server 2019+ | EF Core (Code-First, Migrations)
- **Auth**: ASP.NET Identity
- **Frontend**: Razor Views, Bootstrap 5, jQuery
- **Tooling**: .NET 8 SDK, Git/GitHub

---

## ⚙️ Local Setup (Windows / macOS)

### 1) Clone
```bash
git clone https://github.com/Masavi99/LMS.git
cd LMS
```

### 2) Configure connection string

Create/update appsettings.json:
```
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SQL_SERVER;Database=LMS;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*"
}
```
3) Apply EF Core migrations
dotnet tool restore
```
dotnet ef database update
```

If you need to generate from scratch:
```
dotnet ef migrations add InitialCreate
dotnet ef database update
```
