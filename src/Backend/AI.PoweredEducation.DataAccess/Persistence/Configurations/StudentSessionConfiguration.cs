using AI.PoweredEducation.Entity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AI.PoweredEducation.DataAccess.Persistence.Configurations;

public sealed class StudentSessionConfiguration : IEntityTypeConfiguration<StudentSession>
{
    public void Configure(EntityTypeBuilder<StudentSession> builder)
    {
        builder.ToTable("StudentSessions");
        builder.HasKey(session => session.Id);

        builder.Property(session => session.CreatedAt).IsRequired();
        builder.Property(session => session.StudentName).IsRequired();
        builder.Property(session => session.NormalizedStudentName).IsRequired();
        builder.Property(session => session.SessionTokenHash)
            .IsRequired()
            .HasMaxLength(64);
        builder.Property(session => session.StartedAt).IsRequired();

        builder.HasIndex(session => new { session.LearningGameId, session.FinishedAt });
        builder.HasIndex(session => new { session.LearningGameId, session.NormalizedStudentName })
            .IsUnique()
            .HasFilter("\"FinishedAt\" IS NULL");
        builder.HasIndex(session => session.SessionTokenHash)
            .IsUnique();

        builder.HasMany(session => session.TaskAttempts)
            .WithOne(attempt => attempt.StudentSession)
            .HasForeignKey(attempt => attempt.StudentSessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(session => session.Result)
            .WithOne(result => result.StudentSession)
            .HasForeignKey<Result>(result => result.StudentSessionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
