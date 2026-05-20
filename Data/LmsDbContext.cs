using LMS_SoulCode.Features.Auth.Models;
using LMS_SoulCode.Features.Course.Models;
using LMS_SoulCode.Features.CourseVideos.Models;
using Microsoft.EntityFrameworkCore;
using LMS_SoulCode.Features.SubscribedCourse.Models;
using LMS_SoulCode.Features.Certificates.Models;
using LMS_SoulCode.Features.UserPermissions.Models;
using LMS_SoulCode.Features.Organizations.Models;
using LMS_SoulCode.Features.Groups.Models;
using LMS_SoulCode.Features.Attendance.Models;
using Microsoft.AspNetCore.Http;
using LMS_SoulCode.Features.Common.Models;
using LMS_SoulCode.Features.Common;
using System.Linq.Expressions;

namespace LMS_SoulCode.Data
{
    public class LmsDbContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public int? CurrentTenantId { get; }

        public LmsDbContext(DbContextOptions<LmsDbContext> options, IHttpContextAccessor httpContextAccessor = null)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
            
            var tenantIdClaim = _httpContextAccessor?.HttpContext?.User?.FindFirst("TenantId");
            if (tenantIdClaim != null && int.TryParse(tenantIdClaim.Value, out int tid) && tid != 0)
            {
                CurrentTenantId = tid;
            }
            else
            {
                CurrentTenantId = null;
            }
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseVideo> CourseVideos { get; set; }
        public DbSet<CourseDocument> CourseDocuments { get; set; }
        public DbSet<UserCourse> UserCourses { get; set; } = null!;
        public DbSet<UserVideoProgress> UserVideoProgresses { get; set; }
        public DbSet<Certificate> Certificates { get; set; }
        public DbSet<CertificateTemplate> CertificateTemplates { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RoleModule> RoleModules { get; set; }
        public DbSet<RoleModulePermission> RoleModulePermissions { get; set; }
        public DbSet<ModulePermission> ModulePermissions { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupCourse> GroupCourses { get; set; }
        public DbSet<Attendance> Attendances { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure UserCourse composite primary key
            modelBuilder.Entity<UserCourse>()
                .HasKey(uc => new { uc.UserId, uc.CourseId });

            // ---------------------------------------------------------
            // EXPLICIT GLOBAL QUERY FILTERS (Corretly Handles Multi-Tenancy per Request)
            // ---------------------------------------------------------
            
            // 1. Entities with Multi-Tenancy AND Soft-Delete (BaseTenantEntity)
            modelBuilder.Entity<Attendance>().HasQueryFilter(e => (CurrentTenantId == null || e.TenantId == CurrentTenantId || e.TenantId == 0 || e.TenantId == null) && !e.IsDeleted);
            modelBuilder.Entity<User>().HasQueryFilter(e => (CurrentTenantId == null || e.TenantId == CurrentTenantId || e.TenantId == 0 || e.TenantId == null) && !e.IsDeleted);
            modelBuilder.Entity<Category>().HasQueryFilter(e => (CurrentTenantId == null || e.TenantId == CurrentTenantId || e.TenantId == 0 || e.TenantId == null) && !e.IsDeleted);
            modelBuilder.Entity<Course>().HasQueryFilter(e => (CurrentTenantId == null || e.TenantId == CurrentTenantId || e.TenantId == 0 || e.TenantId == null) && !e.IsDeleted);
            modelBuilder.Entity<CourseVideo>().HasQueryFilter(e => (CurrentTenantId == null || e.TenantId == CurrentTenantId || e.TenantId == 0 || e.TenantId == null) && !e.IsDeleted);
            modelBuilder.Entity<CourseDocument>().HasQueryFilter(e => (CurrentTenantId == null || e.TenantId == CurrentTenantId || e.TenantId == 0 || e.TenantId == null) && !e.IsDeleted);
            modelBuilder.Entity<UserVideoProgress>().HasQueryFilter(e => (CurrentTenantId == null || e.TenantId == CurrentTenantId || e.TenantId == 0 || e.TenantId == null) && !e.IsDeleted);
            modelBuilder.Entity<UserCourse>().HasQueryFilter(e => (CurrentTenantId == null || e.TenantId == CurrentTenantId || e.TenantId == 0 || e.TenantId == null) && !e.IsDeleted);
            modelBuilder.Entity<Certificate>().HasQueryFilter(e => (CurrentTenantId == null || e.TenantId == CurrentTenantId || e.TenantId == 0 || e.TenantId == null) && !e.IsDeleted);
            modelBuilder.Entity<CertificateTemplate>().HasQueryFilter(e => (CurrentTenantId == null || e.TenantId == CurrentTenantId || e.TenantId == 0 || e.TenantId == null) && !e.IsDeleted);
            
            modelBuilder.Entity<Role>().HasQueryFilter(e => (CurrentTenantId == null || e.TenantId == CurrentTenantId || e.TenantId == 0 || e.TenantId == null) && !e.IsDeleted);
            modelBuilder.Entity<UserRole>().HasQueryFilter(e => (CurrentTenantId == null || e.TenantId == CurrentTenantId || e.TenantId == 0 || e.TenantId == null) && !e.IsDeleted);
            modelBuilder.Entity<RoleModule>().HasQueryFilter(e => (CurrentTenantId == null || e.TenantId == CurrentTenantId || e.TenantId == 0 || e.TenantId == null) && !e.IsDeleted);
            modelBuilder.Entity<RoleModulePermission>().HasQueryFilter(e => (CurrentTenantId == null || e.TenantId == CurrentTenantId || e.TenantId == 0 || e.TenantId == null) && !e.IsDeleted);
            modelBuilder.Entity<ModulePermission>().HasQueryFilter(e => (CurrentTenantId == null || e.TenantId == CurrentTenantId || e.TenantId == 0 || e.TenantId == null) && !e.IsDeleted);
            modelBuilder.Entity<Group>().HasQueryFilter(e => (CurrentTenantId == null || e.TenantId == CurrentTenantId || e.TenantId == 0 || e.TenantId == null) && !e.IsDeleted);
            modelBuilder.Entity<GroupCourse>().HasQueryFilter(e => (CurrentTenantId == null || e.TenantId == CurrentTenantId || e.TenantId == 0 || e.TenantId == null) && !e.IsDeleted);

            // 2. Entities with ONLY Soft-Delete (Has IsDeleted but NO TenantId)
            modelBuilder.Entity<Organization>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Module>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Permission>().HasQueryFilter(e => !e.IsDeleted);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                // Handle Auditing
                if (entry.Entity is IAuditEntity auditEntity)
                {
                    if (entry.State == EntityState.Added)
                    {
                        auditEntity.CreatedAt = DateTime.UtcNow;
                        auditEntity.UpdatedAt = DateTime.UtcNow;
                    }

                    if (entry.State == EntityState.Modified)
                    {
                        auditEntity.UpdatedAt = DateTime.UtcNow;
                    }
                }

                // Handle Soft Delete
                if (entry.Entity is ISoftDelete softDeleteEntity && entry.State == EntityState.Modified)
                {
                    var isDeletedProp = entry.Property(nameof(ISoftDelete.IsDeleted));
                    if (isDeletedProp.IsModified)
                    {
                        if (softDeleteEntity.IsDeleted && softDeleteEntity.DeletedAt == null)
                        {
                            softDeleteEntity.DeletedAt = DateTime.UtcNow;
                        }
                        else if (!softDeleteEntity.IsDeleted && softDeleteEntity.DeletedAt != null)
                        {
                            softDeleteEntity.DeletedAt = null;
                        }
                    }
                }


            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
