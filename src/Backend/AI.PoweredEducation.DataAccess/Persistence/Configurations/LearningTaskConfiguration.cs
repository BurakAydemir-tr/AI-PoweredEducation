using AI.PoweredEducation.Entity.Entities;
using AI.PoweredEducation.Entity.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AI.PoweredEducation.DataAccess.Persistence.Configurations;

public sealed class LearningTaskConfiguration : IEntityTypeConfiguration<LearningTask>
{
    public void Configure(EntityTypeBuilder<LearningTask> builder)
    {
        builder.ToTable("LearningTasks");
        builder.HasKey(task => task.Id);

        builder.Property(task => task.CreatedAt).IsRequired();

        builder.HasDiscriminator(task => task.TaskType)
            .HasValue<QuizTask>(LearningTaskType.Quiz)
            .HasValue<QrCodeTask>(LearningTaskType.QrCode)
            .HasValue<GpsTask>(LearningTaskType.Gps);

        builder.HasIndex(task => new { task.LearningGameId, task.Order });

        builder.HasMany(task => task.TaskAttempts)
            .WithOne(attempt => attempt.LearningTask)
            .HasForeignKey(attempt => attempt.LearningTaskId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
