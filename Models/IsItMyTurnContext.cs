using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace IsItMyTurnApi.Models
{
    public partial class IsItMyTurnContext : DbContext
    {
        public IsItMyTurnContext()
        {
        }

        public IsItMyTurnContext(DbContextOptions<IsItMyTurnContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Apartments> Apartments { get; set; }
        public virtual DbSet<CompletedShifts> CompletedShifts { get; set; }
        public virtual DbSet<Devices> Devices { get; set; }
        public virtual DbSet<FcmTokens> FcmTokens { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=tcp:careeria-sql.database.windows.net,1433;Initial Catalog=IsItMyTurn;Persist Security Info=False;User ID=SQLAdmin;Password=866462Tm;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Apartments>(entity =>
            {
                entity.HasKey(e => e.ApartmentId);

                entity.Property(e => e.ApartmentName)
                    .IsRequired()
                    .HasMaxLength(5);
            });

            modelBuilder.Entity<CompletedShifts>(entity =>
            {
                entity.HasKey(e => e.ShiftId);

                entity.Property(e => e.Date).HasColumnType("date");

                entity.HasOne(d => d.Apartment)
                    .WithMany(p => p.CompletedShifts)
                    .HasForeignKey(d => d.ApartmentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CompletedShifts_Apartments");
            });

            modelBuilder.Entity<Devices>(entity =>
            {
                entity.HasKey(e => e.DeviceId);

                entity.Property(e => e.DeviceId).HasColumnName("DeviceID");

                entity.Property(e => e.UniqueIdentifier)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<FcmTokens>(entity =>
            {
                entity.HasKey(e => e.TokenId);

                entity.Property(e => e.TokenId).HasColumnName("TokenID");

                entity.Property(e => e.DeviceId).HasColumnName("DeviceID");

                entity.Property(e => e.Token)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.HasOne(d => d.Device)
                    .WithMany(p => p.FcmTokens)
                    .HasForeignKey(d => d.DeviceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_FcmTokens_Devices");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
