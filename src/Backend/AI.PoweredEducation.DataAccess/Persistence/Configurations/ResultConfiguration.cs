using AI.PoweredEducation.Entity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AI.PoweredEducation.DataAccess.Persistence.Configurations;

public sealed class ResultConfiguration : IEntityTypeConfiguration<Result>
{
    public void Configure(EntityTypeBuilder<Result> builder)
    {
        builder.ToTable("Results");
        builder.HasKey(result => result.Id);

        builder.Property(result => result.CreatedAt).IsRequired();
        builder.Property(result => result.PlayedAt).IsRequired();

        builder.HasIndex(result => result.StudentSessionId)
            .IsUnique();
    }
}
