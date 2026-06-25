using AI.PoweredEducation.Entity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AI.PoweredEducation.DataAccess.Persistence.Configurations;

public sealed class LearningGameConfiguration : IEntityTypeConfiguration<LearningGame>
{
    public void Configure(EntityTypeBuilder<LearningGame> builder)
    {
        builder.ToTable("LearningGames");
        builder.HasKey(game => game.Id);

        builder.Property(game => game.CreatedAt).IsRequired();
        builder.Property(game => game.GradeLevel).IsRequired();
        builder.Property(game => game.Subject).IsRequired();
        builder.Property(game => game.Topic).IsRequired();
        builder.Property(game => game.GameCode).IsRequired();

        builder.HasIndex(game => game.TeacherId);
        builder.HasIndex(game => game.GameCode)
            .IsUnique();

        builder.HasOne(game => game.Teacher)
            .WithMany(teacher => teacher.LearningGames)
            .HasForeignKey(game => game.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(game => game.Tasks)
            .WithOne(task => task.LearningGame)
            .HasForeignKey(task => task.LearningGameId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(game => game.StudentSessions)
            .WithOne(session => session.LearningGame)
            .HasForeignKey(session => session.LearningGameId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
