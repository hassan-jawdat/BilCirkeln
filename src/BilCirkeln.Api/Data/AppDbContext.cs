using BilCirkeln.Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BilCirkeln.Api.Data;

// The DbContext is our connection to the database.
// IdentityDbContext adds the Identity tables (users, roles, ...).
public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<CarSubmission> CarSubmissions => Set<CarSubmission>();
    public DbSet<Offer> Offers => Set<Offer>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Must be called first so Identity can configure its tables.
        base.OnModelCreating(builder);

        builder.Entity<CarSubmission>(e =>
        {
            e.Property(c => c.RegistrationNumber).HasMaxLength(10).IsRequired();
            e.Property(c => c.Make).HasMaxLength(50).IsRequired();
            e.Property(c => c.Model).HasMaxLength(50).IsRequired();
            e.Property(c => c.Description).HasMaxLength(1000);
            e.Property(c => c.AskingPrice).HasPrecision(18, 2);

            // Save enums as text so the database is easy to read.
            e.Property(c => c.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(c => c.FuelType).HasConversion<string>().HasMaxLength(20);
            e.Property(c => c.Gearbox).HasConversion<string>().HasMaxLength(20);
            e.Property(c => c.KeyCount).HasConversion<string>().HasMaxLength(20);
            e.Property(c => c.ServiceHistory).HasConversion<string>().HasMaxLength(20);
            e.Property(c => c.InspectionStatus).HasConversion<string>().HasMaxLength(20);
            e.Property(c => c.SellerType).HasConversion<string>().HasMaxLength(20);
            e.Property(c => c.InteriorColor).HasConversion<string>().HasMaxLength(20);
            e.Property(c => c.InteriorMaterial).HasConversion<string>().HasMaxLength(20);

            e.HasOne(c => c.Customer)
                .WithMany(u => u.CarSubmissions)
                .HasForeignKey(c => c.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Offer>(e =>
        {
            e.Property(o => o.Amount).HasPrecision(18, 2);
            e.Property(o => o.Message).HasMaxLength(1000);
            e.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);

            e.HasOne(o => o.CarSubmission)
                .WithMany(c => c.Offers)
                .HasForeignKey(o => o.CarSubmissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Restrict: SQL Server does not allow two cascade paths to the same table.
            e.HasOne(o => o.Staff)
                .WithMany()
                .HasForeignKey(o => o.StaffId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
