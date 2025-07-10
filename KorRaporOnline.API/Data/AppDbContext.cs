using Microsoft.EntityFrameworkCore;
using KorRaporOnline.API.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KorRaporOnline.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<SavedReport> SavedReports { get; set; }
        public DbSet<ReportParameter> ReportParameters { get; set; }
        public DbSet<DatabaseConnection> DatabaseConnections { get; set; }
        public DbSet<ReportExecution> ReportExecutions { get; set; }
        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserID);

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.FullName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.RefreshToken)
                    .HasMaxLength(256);

                entity.HasIndex(e => e.Username)
                    .IsUnique();

                entity.HasIndex(e => e.Email)
                    .IsUnique();
            });

            // Role configuration
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.RoleID);
                entity.Property(e => e.Name).IsRequired(); // RoleName yerine Name kullanıyoruz
                                                           // ... diğer konfigürasyonlar
            });



            // UserRole configuration
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(e => new { e.UserID, e.RoleID });

                entity.HasOne(e => e.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(e => e.UserID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(e => e.RoleID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // SavedReport configuration
            modelBuilder.Entity<SavedReport>(entity =>
            {
                entity.HasKey(e => e.SavedReportID);

                entity.Property(e => e.ReportName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Description)
                    .HasMaxLength(500);

                entity.Property(e => e.QueryDefinition)
                    .IsRequired();

                entity.HasOne(e => e.User)
                    .WithMany(u => u.SavedReports)
                    .HasForeignKey(e => e.UserID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.DatabaseConnection)
                    .WithMany(d => d.SavedReports)
                    .HasForeignKey(e => e.ConnectionID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ReportParameters için düzeltme
            modelBuilder.Entity<ReportParameter>()
                .HasOne(rp => rp.SavedReport)
                .WithMany(sr => sr.ReportParameters)
                .HasForeignKey(rp => rp.SavedReportID);

            // DatabaseConnection configuration
            modelBuilder.Entity<DatabaseConnection>(entity =>
            {
                entity.HasKey(e => e.ConnectionID);

                entity.Property(e => e.ConnectionName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.ServerName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.DatabaseName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.DatabaseConnections)
                    .HasForeignKey(e => e.UserID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ReportExecutions için düzeltme
            modelBuilder.Entity<ReportExecution>()
                .HasOne(re => re.SavedReport)
                .WithMany(sr => sr.ReportExecutions)
                .HasForeignKey(re => re.SavedReportID);
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is BaseEntity && (
                    e.State == EntityState.Added ||
                    e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                var entity = (BaseEntity)entityEntry.Entity;

                if (entityEntry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.UtcNow;
                }

                entity.LastModified = DateTime.UtcNow;
            }
        }
    }
}