using AI.PoweredEducation.Entity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AI.PoweredEducation.DataAccess.Persistence.Configurations;

public sealed class QrCodeTaskConfiguration : IEntityTypeConfiguration<QrCodeTask>
{
    public void Configure(EntityTypeBuilder<QrCodeTask> builder)
    {
        builder.Property(task => task.Instructions).IsRequired();
        builder.Property(task => task.QrPayload).IsRequired();
        builder.HasIndex(task => task.QrPayload).IsUnique();
    }
}
