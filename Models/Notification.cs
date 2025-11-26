using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NotificationService.gRPC.Models
{

    /// <summary>
    /// Entidad que representa una notificación enviada
    /// </summary>
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        [Required]
        public int UserId { get; set; }

        public int? OrderId { get; set; }

        [Required]
        [MaxLength(20)]
        public string NotificationType { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Recipient { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Subject { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Template { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending";

        [MaxLength(500)]
        public string? FailureReason { get; set; }

        public DateTime? SentAt { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? RetryCount { get; set; } = 0;
    }

    /// <summary>
    /// Enum para tipos de notificación
    /// </summary>
    public static class NotificationType
    {
        public const string Email = "Email";
        public const string SMS = "SMS";
        public const string Push = "Push";
    }

    /// <summary>
    /// Enum para estados de notificación
    /// </summary>
    public static class NotificationStatus
    {
        public const string Pending = "Pending";
        public const string Sent = "Sent";
        public const string Failed = "Failed";
    }

    /// <summary>
    /// Templates de notificación
    /// </summary>
    public static class NotificationTemplate
    {
        // Email templates
        public const string OrderConfirmation = "OrderConfirmation";
        public const string PaymentSuccess = "PaymentSuccess";
        public const string PaymentFailed = "PaymentFailed";
        public const string ShippingUpdate = "ShippingUpdate";
        public const string DeliveryConfirmation = "DeliveryConfirmation";
        public const string OrderCancelled = "OrderCancelled";

        // SMS templates
        public const string OrderUpdate = "OrderUpdate";
        public const string DeliveryNotification = "DeliveryNotification";
        public const string PaymentReminder = "PaymentReminder";
    }
}
