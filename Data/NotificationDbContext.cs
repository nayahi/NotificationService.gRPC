using System.Collections.Generic;
using System.Reflection.Emit;
using global::NotificationService.gRPC.Models;
using Microsoft.EntityFrameworkCore;
using NotificationService.gRPC.Models;

namespace NotificationService.gRPC.Data
{
    /// <summary>
    /// Contexto de base de datos para el servicio de notificaciones
    /// </summary>
    public class NotificationDbContext : DbContext
    {
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de la entidad Notification
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notifications");

                entity.HasKey(e => e.NotificationId);

                entity.Property(e => e.NotificationId)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.UserId)
                    .IsRequired();

                entity.Property(e => e.OrderId);

                entity.Property(e => e.NotificationType)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(e => e.Recipient)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(e => e.Subject)
                    .HasMaxLength(255);

                entity.Property(e => e.Message)
                    .IsRequired();

                entity.Property(e => e.Template)
                    .HasMaxLength(100);

                entity.Property(e => e.Status)
                    .HasMaxLength(20)
                    .IsRequired()
                    .HasDefaultValue("Pending");

                entity.Property(e => e.FailureReason)
                    .HasMaxLength(500);

                entity.Property(e => e.SentAt);

                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.RetryCount)
                    .HasDefaultValue(0);

                // Índices
                entity.HasIndex(e => e.UserId)
                    .HasDatabaseName("IX_Notifications_UserId");

                entity.HasIndex(e => e.OrderId)
                    .HasDatabaseName("IX_Notifications_OrderId");

                entity.HasIndex(e => e.NotificationType)
                    .HasDatabaseName("IX_Notifications_NotificationType");

                entity.HasIndex(e => e.Status)
                    .HasDatabaseName("IX_Notifications_Status");

                entity.HasIndex(e => e.CreatedAt)
                    .HasDatabaseName("IX_Notifications_CreatedAt");

                entity.HasIndex(e => new { e.UserId, e.CreatedAt })
                    .HasDatabaseName("IX_Notifications_UserId_CreatedAt");
            });
        }
    }
}
