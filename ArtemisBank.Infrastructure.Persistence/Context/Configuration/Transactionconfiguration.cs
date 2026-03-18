using ArtemisBank.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtemisBank.Infrastructure.Persistence.Context.Configuration
{
    public class Transactionconfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.ToTable("Transactions");
            builder.HasKey(t => t.Id);

            #region properties
            builder.Property(t => t.TransactionDate).IsRequired();
            builder.Property(t => t.Amount).IsRequired().HasPrecision(18, 2);
            builder.Property(t => t.Beneficiary).IsRequired().HasMaxLength(200);
            builder.Property(t => t.Type).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(t => t.Origin).IsRequired().HasMaxLength(200);
            builder.Property(t => t.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
            #endregion

            #region relationships
            builder.HasOne(t => t.SavingsAccount)
               .WithMany(s => s.Transactions)
               .HasForeignKey(t => t.SavingAccountId)
               .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}
