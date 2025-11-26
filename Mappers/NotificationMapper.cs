using ECommerceGRPC.NotificationService;
using global::NotificationService.gRPC.Models;
using NotificationService.gRPC.Models;

namespace NotificationService.gRPC.Mappers
{
    /// <summary>
    /// Mapeador entre entidades Notification y mensajes Protocol Buffers
    /// </summary>
    public static class NotificationMapper
    {
        /// <summary>
        /// Convierte una entidad Notification a NotificationResponse
        /// </summary>
        public static NotificationResponse ToNotificationResponse(Notification notification)
        {
            var response = new NotificationResponse
            {
                NotificationId = notification.NotificationId,
                UserId = notification.UserId,
                OrderId = notification.OrderId ?? 0,
                NotificationType = notification.NotificationType,
                Recipient = notification.Recipient,
                Message = notification.Message,
                Template = notification.Template,
                Status = notification.Status,
                CreatedAt = notification.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            };

            // Campos opcionales
            if (!string.IsNullOrEmpty(notification.Subject))
                response.Subject = notification.Subject;

            if (!string.IsNullOrEmpty(notification.FailureReason))
                response.FailureReason = notification.FailureReason;

            if (notification.SentAt.HasValue)
                response.SentAt = notification.SentAt.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

            return response;
        }

        /// <summary>
        /// Convierte SendEmailRequest a entidad Notification
        /// </summary>
        public static Notification ToNotificationEntity(SendEmailRequest request)
        {
            return new Notification
            {
                UserId = request.UserId,
                OrderId = request.OrderId > 0 ? request.OrderId : null,
                NotificationType = NotificationType.Email,
                Recipient = request.EmailTo,
                Subject = request.Subject,
                Message = request.Body,
                Template = request.Template,
                Status = NotificationStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Convierte SendSMSRequest a entidad Notification
        /// </summary>
        public static Notification ToNotificationEntity(SendSMSRequest request)
        {
            return new Notification
            {
                UserId = request.UserId,
                OrderId = 0,
                NotificationType = NotificationType.SMS,
                Recipient = request.PhoneNumber,
                Message = request.Message,
                Template = request.Template,
                Status = NotificationStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Convierte una lista de Notification a NotificationHistoryResponse
        /// </summary>
        public static NotificationHistoryResponse ToNotificationHistoryResponse(
            List<Notification> notifications,
            int totalCount,
            int pageNumber,
            int pageSize)
        {
            var response = new NotificationHistoryResponse
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            foreach (var notification in notifications)
            {
                response.Notifications.Add(ToNotificationResponse(notification));
            }

            return response;
        }
    }
}
