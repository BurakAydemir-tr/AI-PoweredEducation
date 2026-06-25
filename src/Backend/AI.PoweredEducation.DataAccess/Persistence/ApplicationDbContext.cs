using AI.PoweredEducation.Entity.Common;
using AI.PoweredEducation.Entity.Entities;
using AI.PoweredEducation.Entity.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AI.PoweredEducation.DataAccess.Persistence;

public class ApplicationDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<LearningGame> LearningGames => Set<LearningGame>();

    public DbSet<LearningTask> LearningTasks => Set<LearningTask>();

    public DbSet<QuizTask> QuizTasks => Set<QuizTask>();

    public DbSet<QrCodeTask> QrCodeTasks => Set<QrCodeTask>();

    public DbSet<GpsTask> GpsTasks => Set<GpsTask>();

    public DbSet<StudentSession> StudentSessions => Set<StudentSession>();

    public DbSet<TaskAttempt> TaskAttempts => Set<TaskAttempt>();

    public DbSet<Result> Results => Set<Result>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override int SaveChanges()
    {
        SetAuditFields();
        return base.SaveChanges(acceptAllChangesOnSuccess: true);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        SetAuditFields();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditFields();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess: true, cancellationToken);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        SetAuditFields();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void SetAuditFields()
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = null;
                continue;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Property(entity => entity.CreatedAt).IsModified = false;
                entry.Entity.UpdatedAt = now;
            }
        }
    }
}
