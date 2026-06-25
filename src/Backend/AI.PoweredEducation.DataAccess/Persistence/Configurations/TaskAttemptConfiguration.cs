using AI.PoweredEducation.Entity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AI.PoweredEducation.DataAccess.Persistence.Configurations;

public sealed class TaskAttemptConfiguration : IEntityTypeConfiguration<TaskAttempt>
{
    public void Configure(EntityTypeBuilder<TaskAttempt> builder)
    {
        builder.ToTable("TaskAttempts");
        builder.HasKey(attempt => attempt.Id);

        builder.Property(attempt => attempt.CreatedAt).IsRequired();

        builder.HasIndex(attempt => new { attempt.StudentSessionId, attempt.LearningTaskId })
            .IsUnique();
    }
}
