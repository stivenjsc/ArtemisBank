using ArtemisBank.Infrastructure.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtemisBank.Infrastructure.Identity.Context.Configuration
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.ToTable("Users");
            builder.HasKey(au => au.Id);
            builder.HasIndex(au => au.Cedula).IsUnique();

            #region properties
            builder.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
            builder.Property(u => u.LastName).IsRequired().HasMaxLength(100);
            builder.Property(u => u.Cedula).IsRequired().HasMaxLength(20);
            builder.Property(u => u.Role).IsRequired().HasConversion<string>();
            builder.Property(u => u.IsActive).IsRequired().HasDefaultValue(false);
            builder.Property(u => u.CommerceId).IsRequired(false);
            #endregion

        }
    }
}
