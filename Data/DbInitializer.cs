using Microsoft.EntityFrameworkCore;
using NotificationService.gRPC.Data;
using NotificationService.gRPC.Models;

namespace PaymentService.gRPC.Data
{
    /// <summary>
    /// Inicializador de base de datos con datos de prueba
    /// </summary>
    public static class DbInitializer
    {
        public static async Task InitializeAsync(NotificationDbContext context, ILogger logger)
        {
            try
            {
                // Asegurar que la base de datos esté creada
                logger.LogInformation("Verificando existencia de base de datos...");
                await context.Database.EnsureCreatedAsync();

                // Aplicar migraciones pendientes
                if (context.Database.GetPendingMigrations().Any())
                {
                    logger.LogInformation("Aplicando migraciones pendientes...");
                    await context.Database.MigrateAsync();
                }

                // Verificar si ya existen productos
                if (await context.Notifications.AnyAsync())
                {
                    logger.LogInformation("Base de datos ya contiene usuarios. Omitiendo inicialización.");
                    return;
                }

                logger.LogInformation("Inicializando datos de prueba para PaymentService...");

                // Notificaciones de prueba
                var notifications = new List<Notification>
                {
                    // Email 1: Order Confirmation - Order 1, UserId 2
                    new Notification
                    {
                        UserId = 2,
                        OrderId = 1,
                        NotificationType = NotificationType.Email,
                        Recipient = "juan.perez@email.com",
                        Subject = "Order Confirmation #1",
                        Message = "Your order has been confirmed and is being processed. Total: $1,499.98",
                        Template = NotificationTemplate.OrderConfirmation,
                        Status = NotificationStatus.Sent,
                        SentAt = DateTime.UtcNow.AddDays(-5),
                        CreatedAt = DateTime.UtcNow.AddDays(-5)
                    },

                    // Email 2: Payment Success - Order 1, UserId 2
                    new Notification
                    {
                        UserId = 2,
                        OrderId = 1,
                        NotificationType = NotificationType.Email,
                        Recipient = "juan.perez@email.com",
                        Subject = "Payment Successful - Order #1",
                        Message = "Your payment of $1,499.98 has been processed successfully. Transaction ID: TXN-20241120-001",
                        Template = NotificationTemplate.PaymentSuccess,
                        Status = NotificationStatus.Sent,
                        SentAt = DateTime.UtcNow.AddDays(-5).AddMinutes(2),
                        CreatedAt = DateTime.UtcNow.AddDays(-5).AddMinutes(2)
                    },

                    // SMS 1: Order Update - Order 1, UserId 2
                    new Notification
                    {
                        UserId = 2,
                        OrderId = 1,
                        NotificationType = NotificationType.SMS,
                        Recipient = "+50612345678",
                        Message = "Tu pedido #1 ha sido confirmado. Total: $1499.98",
                        Template = NotificationTemplate.OrderUpdate,
                        Status = NotificationStatus.Sent,
                        SentAt = DateTime.UtcNow.AddDays(-5).AddMinutes(3),
                        CreatedAt = DateTime.UtcNow.AddDays(-5).AddMinutes(3)
                    },

                    // Email 3: Order Confirmation - Order 2, UserId 3
                    new Notification
                    {
                        UserId = 3,
                        OrderId = 2,
                        NotificationType = NotificationType.Email,
                        Recipient = "maria.gonzalez@email.com",
                        Subject = "Order Confirmation #2",
                        Message = "Your order has been confirmed. Total: $989.97. Expected delivery in 3-5 business days.",
                        Template = NotificationTemplate.OrderConfirmation,
                        Status = NotificationStatus.Sent,
                        SentAt = DateTime.UtcNow.AddDays(-2),
                        CreatedAt = DateTime.UtcNow.AddDays(-2)
                    },

                    // Email 4: Shipping Update - Order 2, UserId 3
                    new Notification
                    {
                        UserId = 3,
                        OrderId = 2,
                        NotificationType = NotificationType.Email,
                        Recipient = "maria.gonzalez@email.com",
                        Subject = "Shipping Update - Order #2",
                        Message = "Your order #2 has been shipped! Tracking number: TRACK123456",
                        Template = NotificationTemplate.ShippingUpdate,
                        Status = NotificationStatus.Sent,
                        SentAt = DateTime.UtcNow.AddDays(-1),
                        CreatedAt = DateTime.UtcNow.AddDays(-1)
                    },

                    // Email 5: Payment Failed - Order 4, UserId 1 (para testing)
                    new Notification
                    {
                        UserId = 1,
                        OrderId = 4,
                        NotificationType = NotificationType.Email,
                        Recipient = "admin@ecommerce.com",
                        Subject = "Payment Failed - Order #4",
                        Message = "We were unable to process your payment. Reason: Insufficient funds. Please update your payment method.",
                        Template = NotificationTemplate.PaymentFailed,
                        Status = NotificationStatus.Sent,
                        SentAt = DateTime.UtcNow.AddDays(-10),
                        CreatedAt = DateTime.UtcNow.AddDays(-10)
                    },

                    // Email 6: Pending - Order 3, UserId 2 (PARA PRUEBAS INMEDIATAS)
                    new Notification
                    {
                        UserId = 2,
                        OrderId = 3,
                        NotificationType = NotificationType.Email,
                        Recipient = "juan.perez@email.com",
                        Subject = "Order Confirmation #3",
                        Message = "Your order has been received and is pending payment confirmation.",
                        Template = NotificationTemplate.OrderConfirmation,
                        Status = NotificationStatus.Pending,
                        CreatedAt = DateTime.UtcNow.AddHours(-2)
                    },

                    // Email 7: Failed notification (para testing de retry)
                    new Notification
                    {
                        UserId = 3,
                        OrderId = 2,
                        NotificationType = NotificationType.Email,
                        Recipient = "invalid-email@",
                        Subject = "Test Failed Notification",
                        Message = "This notification failed to send",
                        Template = NotificationTemplate.OrderConfirmation,
                        Status = NotificationStatus.Failed,
                        FailureReason = "Invalid email address format",
                        CreatedAt = DateTime.UtcNow.AddHours(-3),
                        RetryCount = 2
                    },

                    // SMS 2: Delivery Notification - Order 1, UserId 2
                    new Notification
                    {
                        UserId = 2,
                        OrderId = 1,
                        NotificationType = NotificationType.SMS,
                        Recipient = "+50612345678",
                        Message = "Tu pedido #1 ha sido entregado exitosamente. Gracias por tu compra!",
                        Template = NotificationTemplate.DeliveryNotification,
                        Status = NotificationStatus.Sent,
                        SentAt = DateTime.UtcNow.AddDays(-3),
                        CreatedAt = DateTime.UtcNow.AddDays(-3)
                    }
                };

                await context.Notifications.AddRangeAsync(notifications);
                await context.SaveChangesAsync();

                logger.LogInformation("✓ {Count} notificaciones de prueba creadas exitosamente", notifications.Count);
                logger.LogInformation("  • Email: Order Confirmation (Order 1) - Enviado");
                logger.LogInformation("  • Email: Payment Success (Order 1) - Enviado");
                logger.LogInformation("  • SMS: Order Update (Order 1) - Enviado");
                logger.LogInformation("  • Email: Shipping Update (Order 2) - Enviado");
                logger.LogInformation("  • Email: Payment Failed (Order 4) - Enviado");
                logger.LogInformation("  • Email: Order 3 - Pendiente (PARA PRUEBAS)");
                logger.LogInformation("  • Email: Failed notification - Para testing de retry");
                logger.LogInformation("  • SMS: Delivery Notification (Order 1) - Enviado");

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al inicializar la base de datos de notificaciones");
                throw;
            }
        }
    }
}