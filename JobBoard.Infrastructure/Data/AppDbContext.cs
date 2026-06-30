using JobBoard.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace JobBoard.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // User & Profile Related
        public DbSet<User> Users => Set<User>();
        public DbSet<CandidateProfile> CandidateProfiles => Set<CandidateProfile>();
        public DbSet<WorkExperience> WorkExperiences => Set<WorkExperience>();
        public DbSet<Education> Educations => Set<Education>();
        public DbSet<CandidateSkill> CandidateSkills => Set<CandidateSkill>();
        public DbSet<CandidateLanguage> CandidateLanguages => Set<CandidateLanguage>();
        public DbSet<Portfolio> Portfolios => Set<Portfolio>();

        // Company & Job Related
        public DbSet<Company> Companies => Set<Company>();
        public DbSet<Job> Jobs => Set<Job>();
        public DbSet<JobSkill> JobSkills => Set<JobSkill>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<JobApplication> JobApplications => Set<JobApplication>();
        public DbSet<SavedJob> SavedJobs => Set<SavedJob>();
        public DbSet<JobAlert> JobAlerts => Set<JobAlert>();
        public DbSet<CompanyReview> CompanyReviews => Set<CompanyReview>();

        // Blog Related
        public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
        public DbSet<BlogComment> BlogComments => Set<BlogComment>();

        // Communication & Transactions
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<NewsletterSubscriber> NewsletterSubscribers => Set<NewsletterSubscriber>();
        public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();

        // Chat
        public DbSet<ChatConversation> ChatConversations => Set<ChatConversation>();
        public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

        // Partners (homepage logos)
        public DbSet<Partner> Partners => Set<Partner>();

        // Testimonials (homepage reviews)
        public DbSet<Testimonial> Testimonials => Set<Testimonial>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==================== GLOBAL SOFT DELETE FILTERS ====================
            modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<Job>().HasQueryFilter(j => !j.IsDeleted);
            modelBuilder.Entity<Company>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<BlogPost>().HasQueryFilter(b => !b.IsDeleted);
            modelBuilder.Entity<BlogComment>().HasQueryFilter(bc => !bc.IsDeleted);

            // ==================== UNIQUE CONSTRAINTS ====================
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email_Unique");

            modelBuilder.Entity<Job>()
                .HasIndex(j => j.Slug)
                .IsUnique()
                .HasDatabaseName("IX_Jobs_Slug_Unique");

            modelBuilder.Entity<BlogPost>()
                .HasIndex(b => b.Slug)
                .IsUnique()
                .HasDatabaseName("IX_BlogPosts_Slug_Unique");

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Slug)
                .IsUnique()
                .HasDatabaseName("IX_Categories_Slug_Unique");

            modelBuilder.Entity<NewsletterSubscriber>()
                .HasIndex(n => n.Email)
                .IsUnique()
                .HasDatabaseName("IX_NewsletterSubscribers_Email_Unique");

            modelBuilder.Entity<SiteSetting>()
                .HasIndex(s => s.Key)
                .IsUnique()
                .HasDatabaseName("IX_SiteSettings_Key_Unique");

            modelBuilder.Entity<CandidateProfile>()
                .HasIndex(cp => cp.UserId)
                .IsUnique()
                .HasDatabaseName("IX_CandidateProfiles_UserId_Unique");

            modelBuilder.Entity<Company>()
                .HasIndex(c => c.UserId)
                .IsUnique()
                .HasDatabaseName("IX_Companies_UserId_Unique");

            // ==================== COMPOSITE UNIQUE CONSTRAINTS ====================
            modelBuilder.Entity<SavedJob>()
                .HasIndex(s => new { s.UserId, s.JobId })
                .IsUnique()
                .HasDatabaseName("IX_SavedJobs_UserId_JobId_Unique");

            modelBuilder.Entity<JobApplication>()
                .HasIndex(ja => new { ja.UserId, ja.JobId })
                .IsUnique()
                .HasDatabaseName("IX_JobApplications_UserId_JobId_Unique");

            // ==================== DECIMAL PRECISION ====================
            modelBuilder.Entity<Job>()
                .Property(j => j.SalaryMin)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Job>()
                .Property(j => j.SalaryMax)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasPrecision(18, 2);

            // ==================== STRING ARRAY CONVERSIONS ====================
            modelBuilder.Entity<Portfolio>()
                .Property(p => p.Technologies)
                .HasConversion(
                    v => v == null ? string.Empty : string.Join(";", v),
                    v => string.IsNullOrEmpty(v) ? new string[0] : v.Split(";", StringSplitOptions.RemoveEmptyEntries));

            modelBuilder.Entity<BlogPost>()
                .Property(b => b.Tags)
                .HasConversion(
                    v => v == null ? string.Empty : string.Join(";", v),
                    v => string.IsNullOrEmpty(v) ? new string[0] : v.Split(";", StringSplitOptions.RemoveEmptyEntries));

            // ==================== RELATIONSHIP CONFIGURATIONS ====================

            // User -> CandidateProfile (One-to-One)
            modelBuilder.Entity<CandidateProfile>()
                .HasOne(cp => cp.User)
                .WithOne()
                .HasForeignKey<CandidateProfile>(cp => cp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> Company (One-to-One)
            modelBuilder.Entity<Company>()
                .HasOne(c => c.User)
                .WithOne()
                .HasForeignKey<Company>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // CandidateProfile -> WorkExperience (One-to-Many)
            modelBuilder.Entity<WorkExperience>()
                .HasOne(we => we.CandidateProfile)
                .WithMany(cp => cp.WorkExperiences)
                .HasForeignKey(we => we.CandidateProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // CandidateProfile -> Education (One-to-Many)
            modelBuilder.Entity<Education>()
                .HasOne(e => e.CandidateProfile)
                .WithMany(cp => cp.Educations)
                .HasForeignKey(e => e.CandidateProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // CandidateProfile -> CandidateSkill (One-to-Many)
            modelBuilder.Entity<CandidateSkill>()
                .HasOne(cs => cs.CandidateProfile)
                .WithMany(cp => cp.Skills)
                .HasForeignKey(cs => cs.CandidateProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // CandidateProfile -> CandidateLanguage (One-to-Many)
            modelBuilder.Entity<CandidateLanguage>()
                .HasOne(cl => cl.CandidateProfile)
                .WithMany(cp => cp.Languages)
                .HasForeignKey(cl => cl.CandidateProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // CandidateProfile -> Portfolio (One-to-Many)
            modelBuilder.Entity<Portfolio>()
                .HasOne(p => p.CandidateProfile)
                .WithMany(cp => cp.Portfolios)
                .HasForeignKey(p => p.CandidateProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // Job -> JobSkill (One-to-Many)
            modelBuilder.Entity<JobSkill>()
                .HasOne(js => js.Job)
                .WithMany(j => j.RequiredSkills)
                .HasForeignKey(js => js.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            // Company -> Job (One-to-Many)
            modelBuilder.Entity<Job>()
                .HasOne(j => j.Company)
                .WithMany(c => c.Jobs)
                .HasForeignKey(j => j.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Category -> Job (One-to-Many)
            modelBuilder.Entity<Job>()
                .HasOne(j => j.Category)
                .WithMany(cat => cat.Jobs)
                .HasForeignKey(j => j.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Job -> JobApplication (One-to-Many)
            modelBuilder.Entity<JobApplication>()
                .HasOne(ja => ja.Job)
                .WithMany(j => j.Applications)
                .HasForeignKey(ja => ja.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> JobApplication (One-to-Many)
            modelBuilder.Entity<JobApplication>()
                .HasOne(ja => ja.User)
                .WithMany(u => u.Applications)
                .HasForeignKey(ja => ja.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // User -> SavedJob (One-to-Many)
            modelBuilder.Entity<SavedJob>()
                .HasOne(sj => sj.User)
                .WithMany(u => u.SavedJobs)
                .HasForeignKey(sj => sj.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Job -> SavedJob (One-to-Many)
            modelBuilder.Entity<SavedJob>()
                .HasOne(sj => sj.Job)
                .WithMany(j => j.SavedByUsers)
                .HasForeignKey(sj => sj.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> JobAlert (One-to-Many)
            modelBuilder.Entity<JobAlert>()
                .HasOne(ja => ja.User)
                .WithMany(u => u.JobAlerts)
                .HasForeignKey(ja => ja.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Company -> CompanyReview (One-to-Many)
            modelBuilder.Entity<CompanyReview>()
                .HasOne(cr => cr.Company)
                .WithMany(c => c.Reviews)
                .HasForeignKey(cr => cr.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> CompanyReview (One-to-Many) - NO ACTION to prevent cascade path conflict
            modelBuilder.Entity<CompanyReview>()
                .HasOne(cr => cr.User)
                .WithMany()
                .HasForeignKey(cr => cr.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // User -> BlogPost (One-to-Many)
            modelBuilder.Entity<BlogPost>()
                .HasOne(bp => bp.Author)
                .WithMany()
                .HasForeignKey(bp => bp.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);

            // BlogPost -> BlogComment (One-to-Many)
            modelBuilder.Entity<BlogComment>()
                .HasOne(bc => bc.BlogPost)
                .WithMany(bp => bp.Comments)
                .HasForeignKey(bc => bc.BlogPostId)
                .OnDelete(DeleteBehavior.NoAction);

            // User -> BlogComment (One-to-Many)
            modelBuilder.Entity<BlogComment>()
                .HasOne(bc => bc.User)
                .WithMany()
                .HasForeignKey(bc => bc.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // BlogComment -> BlogComment (Self-referencing, One-to-Many)
            modelBuilder.Entity<BlogComment>()
                .HasOne(bc => bc.ParentComment)
                .WithMany(bc => bc.Replies)
                .HasForeignKey(bc => bc.ParentCommentId)
                .OnDelete(DeleteBehavior.NoAction);

            // User -> Transaction (One-to-Many)
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.User)
                .WithMany(u => u.Transactions)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> Notification (One-to-Many)
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ChatConversation -> ChatMessage (One-to-Many)
            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChatConversation>()
                .HasIndex(c => c.ApplicationId)
                .HasDatabaseName("IX_ChatConversations_ApplicationId");
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State is EntityState.Added or EntityState.Modified);

            foreach (var entry in entries)
            {
                switch (entry.Entity)
                {
                    case User user:
                        if (entry.State == EntityState.Added)
                            user.CreatedAt = DateTime.UtcNow;
                        user.UpdatedAt = DateTime.UtcNow;
                        break;

                    case Job job:
                        if (entry.State == EntityState.Added)
                            job.CreatedAt = DateTime.UtcNow;
                        job.UpdatedAt = DateTime.UtcNow;
                        break;

                    case Company company:
                        if (entry.State == EntityState.Added)
                            company.CreatedAt = DateTime.UtcNow;
                        company.UpdatedAt = DateTime.UtcNow;
                        break;

                    case BlogPost blogPost:
                        if (entry.State == EntityState.Added)
                            blogPost.CreatedAt = DateTime.UtcNow;
                        blogPost.UpdatedAt = DateTime.UtcNow;
                        break;

                    case JobApplication jobApplication:
                        if (entry.State == EntityState.Added)
                            jobApplication.AppliedAt = DateTime.UtcNow;
                        jobApplication.UpdatedAt = DateTime.UtcNow;
                        break;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}