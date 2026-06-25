using AI.PoweredEducation.Entity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AI.PoweredEducation.DataAccess.Persistence.Configurations;

public sealed class QuizTaskConfiguration : IEntityTypeConfiguration<QuizTask>
{
    public void Configure(EntityTypeBuilder<QuizTask> builder)
    {
        builder.Property(task => task.Question).IsRequired();
        builder.Property(task => task.OptionA).IsRequired();
        builder.Property(task => task.OptionB).IsRequired();
        builder.Property(task => task.OptionC).IsRequired();
        builder.Property(task => task.OptionD).IsRequired();
    }
}
