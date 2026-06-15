using JobBoard.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Infrastructure.Data
{

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<CandidateProfile> CandidateProfiles => Set<CandidateProfile>();
        public DbSet<Company> Companies => Set<Company>();
        public DbSet<Job> Jobs => Set<Job>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<JobApplication> JobApplications => Set<JobApplication>();
        public DbSet<WorkExperience> WorkExperiences => Set<WorkExperience>();
        public DbSet<Education> Educations => Set<Education>();
        public DbSet<CandidateSkill> CandidateSkills => Set<CandidateSkill>();
        public DbSet<JobSkill> JobSkills => Set<JobSkill>();
        public DbSet<CandidateLanguage> CandidateLanguages => Set<CandidateLanguage>();
        public DbSet<SavedJob> SavedJobs => Set<SavedJob>();
        public DbSet<JobAlert> JobAlerts => Set<JobAlert>();
        public DbSet<Portfolio> Portfolios => Set<Portfolio>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
        public DbSet<BlogComment> BlogComments => Set<BlogComment>();
        public DbSet<CompanyReview> CompanyReviews => Set<CompanyReview>();
        public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
        public DbSet<NewsletterSubscriber> NewsletterSubscribers => Set<NewsletterSubscriber>();
        public DbSet<Notification> Notifications => Set<Notification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Global soft delete filters
            modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<Job>().HasQueryFilter(j => !j.IsDeleted);
            modelBuilder.Entity<Company>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<BlogPost>().HasQueryFilter(b => !b.IsDeleted);
            modelBuilder.Entity<BlogComment>().HasQueryFilter(c => !c.IsDeleted);

            // Unique indexes
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<Job>().HasIndex(j => j.Slug).IsUnique();
            modelBuilder.Entity<BlogPost>().HasIndex(b => b.Slug).IsUnique();
            modelBuilder.Entity<Category>().HasIndex(c => c.Slug).IsUnique();
            modelBuilder.Entity<NewsletterSubscriber>().HasIndex(n => n.Email).IsUnique();

            // SavedJob — composite unique
            modelBuilder.Entity<SavedJob>()
                .HasIndex(s => new { s.UserId, s.JobId }).IsUnique();

            // JobApplication — composite unique
            modelBuilder.Entity<JobApplication>()
                .HasIndex(a => new { a.UserId, a.JobId }).IsUnique();

            // Decimal precision
            modelBuilder.Entity<Job>()
                .Property(j => j.SalaryMin).HasPrecision(18, 2);
            modelBuilder.Entity<Job>()
                .Property(j => j.SalaryMax).HasPrecision(18, 2);
            modelBuilder.Entity<Transaction>()
                .Property(t => t.Amount).HasPrecision(18, 2);

            // Array type for PostgreSQL (SQL Server üçün JSON string)
            modelBuilder.Entity<Portfolio>()
                .Property(p => p.Technologies)
                .HasConversion(
                    v => v == null ? null : string.Join(",", v),
                    v => v == null ? null : v.Split(",", StringSplitOptions.RemoveEmptyEntries));

            modelBuilder.Entity<BlogPost>()
                .Property(b => b.Tags)
                .HasConversion(
                    v => v == null ? null : string.Join(",", v),
                    v => v == null ? null : v.Split(",", StringSplitOptions.RemoveEmptyEntries));
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State is EntityState.Added or EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.Entity is User u)
                {
                    if (entry.State == EntityState.Added) u.CreatedAt = DateTime.UtcNow;
                    u.UpdatedAt = DateTime.UtcNow;
                }
                if (entry.Entity is Job j)
                {
                    if (entry.State == EntityState.Added) j.CreatedAt = DateTime.UtcNow;
                    j.UpdatedAt = DateTime.UtcNow;
                }
                if (entry.Entity is Company c)
                {
                    if (entry.State == EntityState.Added) c.CreatedAt = DateTime.UtcNow;
                    c.UpdatedAt = DateTime.UtcNow;
                }
                if (entry.Entity is BlogPost b)
                {
                    if (entry.State == EntityState.Added) b.CreatedAt = DateTime.UtcNow;
                    b.UpdatedAt = DateTime.UtcNow;
                }
                if (entry.Entity is JobApplication a)
                {
                    if (entry.State == EntityState.Added) a.AppliedAt = DateTime.UtcNow;
                    a.UpdatedAt = DateTime.UtcNow;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}