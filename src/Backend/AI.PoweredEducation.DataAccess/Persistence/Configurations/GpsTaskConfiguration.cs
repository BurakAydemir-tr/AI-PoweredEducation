using AI.PoweredEducation.Entity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AI.PoweredEducation.DataAccess.Persistence.Configurations;

public sealed class GpsTaskConfiguration : IEntityTypeConfiguration<GpsTask>
{
    public void Configure(EntityTypeBuilder<GpsTask> builder)
    {
        builder.Property(task => task.Instructions).IsRequired();
        builder.Property(task => task.GameAreaJson)
            .IsRequired()
            .HasColumnType("jsonb");
    }
}
