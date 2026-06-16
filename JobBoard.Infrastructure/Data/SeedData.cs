using JobBoard.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Infrastructure.Data
{

    public static class SeedData
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            if (await db.Categories.AnyAsync()) return;

            // --- Kateqoriyalar ---
            var categories = new List<Category>
        {
            new() { Name = "Technology",  Slug = "technology",  IconClass = "fas fa-laptop-code", Color = "#6366f1", SortOrder = 1 },
            new() { Name = "Marketing",   Slug = "marketing",   IconClass = "fas fa-bullhorn",     Color = "#f59e0b", SortOrder = 2 },
            new() { Name = "Finance",     Slug = "finance",     IconClass = "fas fa-chart-line",   Color = "#10b981", SortOrder = 3 },
            new() { Name = "Healthcare",  Slug = "healthcare",  IconClass = "fas fa-heartbeat",    Color = "#ef4444", SortOrder = 4 },
            new() { Name = "Education",   Slug = "education",   IconClass = "fas fa-graduation-cap",Color = "#8b5cf6",SortOrder = 5 },
            new() { Name = "Design",      Slug = "design",      IconClass = "fas fa-palette",      Color = "#ec4899", SortOrder = 6 },
            new() { Name = "Engineering", Slug = "engineering", IconClass = "fas fa-cogs",         Color = "#14b8a6", SortOrder = 7 },
            new() { Name = "Sales",       Slug = "sales",       IconClass = "fas fa-handshake",    Color = "#f97316", SortOrder = 8 },
            new() { Name = "Legal",       Slug = "legal",       IconClass = "fas fa-balance-scale", Color = "#6b7280",SortOrder = 9 },
            new() { Name = "HR",          Slug = "hr",          IconClass = "fas fa-users",        Color = "#0ea5e9", SortOrder = 10 }
        };
            db.Categories.AddRange(categories);
            await db.SaveChangesAsync();

            // --- Admin user ---
            var adminUser = new User
            {
                FullName = "Admin User",
                Email = "admin@jobboard.az",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = "admin",
                IsEmailVerified = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            db.Users.Add(adminUser);

            // --- Employer users ---
            var employer1 = new User
            {
                FullName = "Anar Həsənov",
                Email = "employer1@jobboard.az",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@123"),
                Role = "employer",
                IsEmailVerified = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var employer2 = new User
            {
                FullName = "Lalə Məmmədova",
                Email = "employer2@jobboard.az",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@123"),
                Role = "employer",
                IsEmailVerified = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            db.Users.AddRange(employer1, employer2);

            // --- Candidate users ---
            var candidates = Enumerable.Range(1, 5).Select(i => new User
            {
                FullName = $"Namizəd {i}",
                Email = $"candidate{i}@jobboard.az",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@123"),
                Role = "candidate",
                IsEmailVerified = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }).ToList();
            db.Users.AddRange(candidates);

            await db.SaveChangesAsync();

            // --- Companies ---
            var company1 = new Company
            {
                UserId = employer1.Id,
                Name = "TechAz MMC",
                Description = "Azərbaycanda aparıcı IT şirkəti. 2015-ci ildən fəaliyyət göstəririk.",
                Industry = "Technology",
                CompanySize = "51-200",
                Website = "https://techaz.az",
                Location = "Bakı, Azərbaycan",
                IsVerified = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var company2 = new Company
            {
                UserId = employer2.Id,
                Name = "FinStart Startup",
                Description = "Maliyyə texnologiyaları sahəsində innovativ startup.",
                Industry = "Finance",
                CompanySize = "11-50",
                Website = "https://finstart.az",
                Location = "Bakı, Azərbaycan",
                IsVerified = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            db.Companies.AddRange(company1, company2);
            await db.SaveChangesAsync();

            // --- Candidate profiles ---
            var profileData = new[]
            {
            ("Senior .NET Developer", "10+ il təcrübəli backend developer"),
            ("React Frontend Developer", "Modern UI/UX həlləri üzrə mütəxəssis"),
            ("DevOps Engineer", "AWS və Docker üzrə sertifikatlı mühəndis"),
            ("UI/UX Designer", "Figma və Adobe XD ustası"),
            ("Data Scientist", "Machine learning və analitika mütəxəssisi")
        };

            for (int i = 0; i < candidates.Count; i++)
            {
                var profile = new CandidateProfile
                {
                    UserId = candidates[i].Id,
                    Headline = profileData[i].Item1,
                    Summary = profileData[i].Item2,
                    Location = "Bakı, Azərbaycan",
                    ExperienceYears = (i + 1) * 2,
                    IsAvailable = i % 2 == 0
                };
                db.CandidateProfiles.Add(profile);
            }
            await db.SaveChangesAsync();

            // --- Jobs ---
            var techCatId = categories.First(c => c.Slug == "technology").Id;
            var financeCatId = categories.First(c => c.Slug == "finance").Id;
            var designCatId = categories.First(c => c.Slug == "design").Id;

            var jobs = new List<Job>
        {
            new() {
                CompanyId = company1.Id, CategoryId = techCatId,
                Title = "Senior .NET Developer", Slug = "senior-net-developer",
                Description = "ASP.NET Core 8 ilə microservices arxitekturası üzərində işləyəcək təcrübəli developer axtarırıq.",
                Location = "Bakı", IsRemote = false, JobType = "full-time",
                ExperienceLevel = "senior", SalaryMin = 3000, SalaryMax = 5000,
                SalaryCurrency = "USD", SalaryPeriod = "monthly", IsSalaryVisible = true,
                Status = "active", IsFeatured = true, IsUrgent = false,
                Deadline = DateTime.UtcNow.AddMonths(2), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            },
            new() {
                CompanyId = company1.Id, CategoryId = techCatId,
                Title = "React Frontend Developer", Slug = "react-frontend-developer",
                Description = "React və TypeScript ilə müasir web tətbiqləri hazırlayacaq developer.",
                Location = "Bakı", IsRemote = true, JobType = "full-time",
                ExperienceLevel = "mid", SalaryMin = 2000, SalaryMax = 3500,
                SalaryCurrency = "USD", SalaryPeriod = "monthly", IsSalaryVisible = true,
                Status = "active", IsFeatured = true, IsUrgent = true,
                Deadline = DateTime.UtcNow.AddMonths(1), CreatedAt = DateTime.UtcNow.AddDays(-3), UpdatedAt = DateTime.UtcNow
            },
            new() {
                CompanyId = company1.Id, CategoryId = techCatId,
                Title = "DevOps Engineer", Slug = "devops-engineer",
                Description = "CI/CD pipeline, Docker, Kubernetes üzrə təcrübəli mühəndis.",
                Location = "Bakı", IsRemote = true, JobType = "full-time",
                ExperienceLevel = "senior", SalaryMin = 3500, SalaryMax = 6000,
                SalaryCurrency = "USD", SalaryPeriod = "monthly", IsSalaryVisible = true,
                Status = "active", IsFeatured = false, IsUrgent = false,
                Deadline = DateTime.UtcNow.AddMonths(3), CreatedAt = DateTime.UtcNow.AddDays(-5), UpdatedAt = DateTime.UtcNow
            },
            new() {
                CompanyId = company2.Id, CategoryId = financeCatId,
                Title = "Financial Analyst", Slug = "financial-analyst",
                Description = "Maliyyə hesabatları və investisiya analizi üzrə mütəxəssis.",
                Location = "Bakı", IsRemote = false, JobType = "full-time",
                ExperienceLevel = "mid", SalaryMin = 1500, SalaryMax = 2500,
                SalaryCurrency = "USD", SalaryPeriod = "monthly", IsSalaryVisible = false,
                Status = "active", IsFeatured = false, IsUrgent = false,
                Deadline = DateTime.UtcNow.AddMonths(2), CreatedAt = DateTime.UtcNow.AddDays(-7), UpdatedAt = DateTime.UtcNow
            },
            new() {
                CompanyId = company2.Id, CategoryId = financeCatId,
                Title = "Blockchain Developer", Slug = "blockchain-developer",
                Description = "Solidity və Web3 texnologiyaları üzrə DeFi layihələrini inkişaf etdirəcək developer.",
                Location = "Remote", IsRemote = true, JobType = "contract",
                ExperienceLevel = "senior", SalaryMin = 5000, SalaryMax = 8000,
                SalaryCurrency = "USD", SalaryPeriod = "monthly", IsSalaryVisible = true,
                Status = "active", IsFeatured = true, IsUrgent = true,
                Deadline = DateTime.UtcNow.AddMonths(1), CreatedAt = DateTime.UtcNow.AddDays(-1), UpdatedAt = DateTime.UtcNow
            },
            new() {
                CompanyId = company1.Id, CategoryId = designCatId,
                Title = "UI/UX Designer", Slug = "ui-ux-designer",
                Description = "Figma ilə istifadəçi interfeysi dizayn edəcək yaradıcı dizayner.",
                Location = "Bakı", IsRemote = false, JobType = "full-time",
                ExperienceLevel = "mid", SalaryMin = 1800, SalaryMax = 3000,
                SalaryCurrency = "USD", SalaryPeriod = "monthly", IsSalaryVisible = true,
                Status = "active", IsFeatured = false, IsUrgent = false,
                Deadline = DateTime.UtcNow.AddMonths(2), CreatedAt = DateTime.UtcNow.AddDays(-10), UpdatedAt = DateTime.UtcNow
            }
        };
            db.Jobs.AddRange(jobs);
            await db.SaveChangesAsync();

            // --- Job Skills ---
            var jobSkills = new List<JobSkill>
        {
            new() { JobId = jobs[0].Id, Name = "C#" },
            new() { JobId = jobs[0].Id, Name = ".NET Core" },
            new() { JobId = jobs[0].Id, Name = "SQL Server" },
            new() { JobId = jobs[0].Id, Name = "Docker" },
            new() { JobId = jobs[1].Id, Name = "React" },
            new() { JobId = jobs[1].Id, Name = "TypeScript" },
            new() { JobId = jobs[1].Id, Name = "Redux" },
            new() { JobId = jobs[2].Id, Name = "Kubernetes" },
            new() { JobId = jobs[2].Id, Name = "Docker" },
            new() { JobId = jobs[2].Id, Name = "AWS" },
            new() { JobId = jobs[2].Id, Name = "CI/CD" },
            new() { JobId = jobs[3].Id, Name = "Excel" },
            new() { JobId = jobs[3].Id, Name = "Financial Modeling" },
            new() { JobId = jobs[4].Id, Name = "Solidity" },
            new() { JobId = jobs[4].Id, Name = "Web3.js" },
            new() { JobId = jobs[4].Id, Name = "Ethereum" },
            new() { JobId = jobs[5].Id, Name = "Figma" },
            new() { JobId = jobs[5].Id, Name = "Adobe XD" }
        };
            db.JobSkills.AddRange(jobSkills);

            // --- Blog Posts ---
            var blogPosts = new List<BlogPost>
        {
            new() {
                AuthorId = adminUser.Id, Title = "2025-ci ildə ən çox tələb olunan IT bacarıqları",
                Slug = "2025-en-cox-teleb-olunan-it-bacariqlar",
                Content = "Texnologiya dünyası sürətlə dəyişir. Bu il ən çox tələb olunan bacarıqlar...",
                Excerpt = "2025-ci ildə işə qəbulda üstünlük verilən texniki bacarıqlar haqqında ətraflı məlumat.",
                Category = "Career", Status = "published", IsFeatured = true,
                ReadTimeMinutes = 5, Tags = ["IT", "Career", "Skills"],
                PublishedAt = DateTime.UtcNow.AddDays(-2),
                CreatedAt = DateTime.UtcNow.AddDays(-3), UpdatedAt = DateTime.UtcNow
            },
            new() {
                AuthorId = adminUser.Id, Title = "Remote iş: Müvəffəqiyyətin sirləri",
                Slug = "remote-is-muveffeqiyyetin-sirleri",
                Content = "Remote iş modeli artıq əksər şirkətlərin ayrılmaz hissəsinə çevrilib...",
                Excerpt = "Evdən işləyərkən məhsuldarlığı artırmağın praktiki yolları.",
                Category = "Productivity", Status = "published", IsFeatured = false,
                ReadTimeMinutes = 4, Tags = ["Remote", "Productivity", "Work"],
                PublishedAt = DateTime.UtcNow.AddDays(-5),
                CreatedAt = DateTime.UtcNow.AddDays(-6), UpdatedAt = DateTime.UtcNow
            },
            new() {
                AuthorId = adminUser.Id, Title = "CV yazmağın incəlikləri",
                Slug = "cv-yazmaghin-incelikleri",
                Content = "Güclü CV yazmaq üçün bilməli olduğunuz hər şey bu məqalədə...",
                Excerpt = "İşəgötürənlərin diqqətini çəkən CV hazırlamağın praktiki rehbəri.",
                Category = "Career", Status = "published", IsFeatured = true,
                ReadTimeMinutes = 6, Tags = ["CV", "Career", "Tips"],
                PublishedAt = DateTime.UtcNow.AddDays(-10),
                CreatedAt = DateTime.UtcNow.AddDays(-11), UpdatedAt = DateTime.UtcNow
            },
            new() {
                AuthorId = adminUser.Id, Title = "Startup-da işləmək: Üstünlüklər və çətinliklər",
                Slug = "startup-da-islemek",
                Content = "Startup mühiti böyük şirkətlərdən əsaslı şəkildə fərqlənir...",
                Excerpt = "Startup mədəniyyəti, sürətli inkişaf imkanları və risk faktorları.",
                Category = "Industry", Status = "published", IsFeatured = false,
                ReadTimeMinutes = 7, Tags = ["Startup", "Career", "Culture"],
                PublishedAt = DateTime.UtcNow.AddDays(-15),
                CreatedAt = DateTime.UtcNow.AddDays(-16), UpdatedAt = DateTime.UtcNow
            },
            new() {
                AuthorId = adminUser.Id, Title = "Müsahibəyə hazırlıq: 10 vacib məsləhət",
                Slug = "muesahiheye-hazirlig-10-vacib-meslehet",
                Content = "İş müsahibəsindən əvvəl bilməli olduğunuz hər şey...",
                Excerpt = "Müsahibədə uğurlu olmaq üçün praktiki tövsiyələr.",
                Category = "Interview", Status = "published", IsFeatured = false,
                ReadTimeMinutes = 5, Tags = ["Interview", "Tips", "Career"],
                PublishedAt = DateTime.UtcNow.AddDays(-20),
                CreatedAt = DateTime.UtcNow.AddDays(-21), UpdatedAt = DateTime.UtcNow
            }
        };
            db.BlogPosts.AddRange(blogPosts);

            await db.SaveChangesAsync();
        }
    }
}
