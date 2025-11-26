using ECommerceGRPC.NotificationService;
using FluentValidation;
using global::NotificationService.gRPC.Data;
using global::NotificationService.gRPC.Mappers;
using global::NotificationService.gRPC.Models;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using NotificationService.gRPC.Data;
using NotificationService.gRPC.Mappers;
using NotificationService.gRPC.Models;

namespace NotificationService.gRPC.Services
{
    /// <summary>
    /// Implementación del servicio gRPC de notificaciones con simulación de envío
    /// Mock: Los emails y SMS no se envían realmente, solo se registran en base de datos
    /// </summary>
    public class NotificationGrpcService : ECommerceGRPC.NotificationService.NotificationService.NotificationServiceBase
    {
        private readonly NotificationDbContext _context;
        private readonly ILogger<NotificationGrpcService> _logger;
        private readonly IValidator<SendEmailRequest> _emailValidator;
        private readonly IValidator<SendSMSRequest> _smsValidator;
        private readonly IValidator<GetNotificationHistoryRequest> _historyValidator;
        private readonly IValidator<GetNotificationRequest> _getValidator;
        private readonly IValidator<ResendNotificationRequest> _resendValidator;
        private static readonly Random _random = new Random();

        public NotificationGrpcService(
            NotificationDbContext context,
            ILogger<NotificationGrpcService> logger,
            IValidator<SendEmailRequest> emailValidator,
            IValidator<SendSMSRequest> smsValidator,
            IValidator<GetNotificationHistoryRequest> historyValidator,
            IValidator<GetNotificationRequest> getValidator,
            IValidator<ResendNotificationRequest> resendValidator)
        {
            _context = context;
            _logger = logger;
            _emailValidator = emailValidator;
            _smsValidator = smsValidator;
            _historyValidator = historyValidator;
            _getValidator = getValidator;
            _resendValidator = resendValidator;
        }

        /// <summary>
        /// Envía un email (mock)
        /// </summary>
        public override async Task<NotificationResponse> SendEmail(
            SendEmailRequest request,
            ServerCallContext context)
        {
            _logger.LogInformation("Enviando email a {Email}, Asunto: {Subject}",
                request.EmailTo, request.Subject);

            // Validar request
            var validationResult = await _emailValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogWarning("Validación fallida para SendEmail: {Errors}", errors);
                throw new RpcException(new Status(StatusCode.InvalidArgument, errors));
            }

            try
            {
                // Crear entidad de notificación
                var notification = NotificationMapper.ToNotificationEntity(request);

                // Mock: Simular envío de email
                await Task.Delay(_random.Next(100, 500)); // Simular latencia de proveedor de email

                // Mock: 5% de probabilidad de fallo para testing
                var shouldFail = _random.Next(1, 101) <= 5;

                if (shouldFail)
                {
                    notification.Status = NotificationStatus.Failed;
                    notification.FailureReason = GetRandomEmailFailureReason();

                    _logger.LogWarning(
                        "📧 Email FALLIDO (simulado) - To: {Email}, Reason: {Reason}",
                        request.EmailTo, notification.FailureReason);
                }
                else
                {
                    notification.Status = NotificationStatus.Sent;
                    notification.SentAt = DateTime.UtcNow;

                    _logger.LogInformation(
                        "✓ Email enviado exitosamente - To: {Email}, Template: {Template}",
                        request.EmailTo, request.Template);
                }

                // Guardar en base de datos
                await _context.Notifications.AddAsync(notification);
                await _context.SaveChangesAsync();

                return NotificationMapper.ToNotificationResponse(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar email a {Email}", request.EmailTo);
                throw new RpcException(new Status(StatusCode.Internal,
                    $"Error interno al enviar email: {ex.Message}"));
            }
        }

        /// <summary>
        /// Envía un SMS (mock)
        /// </summary>
        public override async Task<NotificationResponse> SendSMS(
            SendSMSRequest request,
            ServerCallContext context)
        {
            _logger.LogInformation("Enviando SMS a {Phone}, Message: {Message}",
                request.PhoneNumber, request.Message.Substring(0, Math.Min(50, request.Message.Length)));

            // Validar request
            var validationResult = await _smsValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogWarning("Validación fallida para SendSMS: {Errors}", errors);
                throw new RpcException(new Status(StatusCode.InvalidArgument, errors));
            }

            try
            {
                // Crear entidad de notificación
                var notification = NotificationMapper.ToNotificationEntity(request);

                // Mock: Simular envío de SMS
                await Task.Delay(_random.Next(50, 300)); // Simular latencia de proveedor SMS

                // Mock: 3% de probabilidad de fallo para testing
                var shouldFail = _random.Next(1, 101) <= 3;

                if (shouldFail)
                {
                    notification.Status = NotificationStatus.Failed;
                    notification.FailureReason = GetRandomSMSFailureReason();

                    _logger.LogWarning(
                        "📱 SMS FALLIDO (simulado) - To: {Phone}, Reason: {Reason}",
                        request.PhoneNumber, notification.FailureReason);
                }
                else
                {
                    notification.Status = NotificationStatus.Sent;
                    notification.SentAt = DateTime.UtcNow;

                    _logger.LogInformation(
                        "✓ SMS enviado exitosamente - To: {Phone}, Template: {Template}",
                        request.PhoneNumber, request.Template);
                }

                // Guardar en base de datos
                await _context.Notifications.AddAsync(notification);
                await _context.SaveChangesAsync();

                return NotificationMapper.ToNotificationResponse(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar SMS a {Phone}", request.PhoneNumber);
                throw new RpcException(new Status(StatusCode.Internal,
                    $"Error interno al enviar SMS: {ex.Message}"));
            }
        }

        /// <summary>
        /// Obtiene historial de notificaciones de un usuario
        /// </summary>
        public override async Task<NotificationHistoryResponse> GetNotificationHistory(
            GetNotificationHistoryRequest request,
            ServerCallContext context)
        {
            _logger.LogInformation("Consultando historial de notificaciones para User {UserId}", request.UserId);

            // Validar request
            var validationResult = await _historyValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new RpcException(new Status(StatusCode.InvalidArgument, errors));
            }

            try
            {
                // Configurar paginación
                var pageNumber = request.PageNumber > 0 ? request.PageNumber : 1;
                var pageSize = request.PageSize > 0 ? request.PageSize : 20;

                // Construir query base
                var query = _context.Notifications
                    .AsNoTracking()
                    .Where(n => n.UserId == request.UserId);

                // Filtrar por tipo si se especifica
                if (!string.IsNullOrEmpty(request.NotificationType))
                {
                    query = query.Where(n => n.NotificationType == request.NotificationType);
                }

                // Obtener total de registros
                var totalCount = await query.CountAsync();

                // Aplicar paginación y ordenar
                var notifications = await query
                    .OrderByDescending(n => n.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                _logger.LogInformation("✓ Encontradas {Count} notificaciones para User {UserId} (Total: {Total})",
                    notifications.Count, request.UserId, totalCount);

                return NotificationMapper.ToNotificationHistoryResponse(
                    notifications, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener historial de notificaciones para User {UserId}", request.UserId);
                throw new RpcException(new Status(StatusCode.Internal,
                    $"Error interno: {ex.Message}"));
            }
        }

        /// <summary>
        /// Obtiene una notificación por ID
        /// </summary>
        public override async Task<NotificationResponse> GetNotification(
            GetNotificationRequest request,
            ServerCallContext context)
        {
            _logger.LogInformation("Consultando Notification {NotificationId}", request.NotificationId);

            // Validar request
            var validationResult = await _getValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new RpcException(new Status(StatusCode.InvalidArgument, errors));
            }

            try
            {
                var notification = await _context.Notifications
                    .AsNoTracking()
                    .FirstOrDefaultAsync(n => n.NotificationId == request.NotificationId);

                if (notification == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound,
                        $"Notificación con ID {request.NotificationId} no encontrada"));
                }

                return NotificationMapper.ToNotificationResponse(notification);
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener Notification {NotificationId}", request.NotificationId);
                throw new RpcException(new Status(StatusCode.Internal,
                    $"Error interno: {ex.Message}"));
            }
        }

        /// <summary>
        /// Reenvía una notificación fallida
        /// </summary>
        public override async Task<NotificationResponse> ResendNotification(
            ResendNotificationRequest request,
            ServerCallContext context)
        {
            _logger.LogInformation("Reenviando Notification {NotificationId}", request.NotificationId);

            // Validar request
            var validationResult = await _resendValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new RpcException(new Status(StatusCode.InvalidArgument, errors));
            }

            try
            {
                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.NotificationId == request.NotificationId);

                if (notification == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound,
                        $"Notificación con ID {request.NotificationId} no encontrada"));
                }

                // Verificar que esté en estado Failed o Pending
                if (notification.Status != NotificationStatus.Failed && notification.Status != NotificationStatus.Pending)
                {
                    throw new RpcException(new Status(StatusCode.FailedPrecondition,
                        $"Solo se pueden reenviar notificaciones Failed o Pending. Estado actual: {notification.Status}"));
                }

                // Mock: Simular reenvío
                await Task.Delay(_random.Next(100, 500));

                // Mock: 20% de probabilidad de fallo en reenvío
                var shouldFail = _random.Next(1, 101) <= 20;

                if (shouldFail)
                {
                    notification.Status = NotificationStatus.Failed;
                    notification.FailureReason = notification.NotificationType == NotificationType.Email
                        ? GetRandomEmailFailureReason()
                        : GetRandomSMSFailureReason();
                    notification.RetryCount = (notification.RetryCount ?? 0) + 1;

                    _logger.LogWarning(
                        "❌ Reenvío FALLIDO - Notification {NotificationId}, Reason: {Reason}",
                        request.NotificationId, notification.FailureReason);
                }
                else
                {
                    notification.Status = NotificationStatus.Sent;
                    notification.SentAt = DateTime.UtcNow;
                    notification.FailureReason = null;
                    notification.RetryCount = (notification.RetryCount ?? 0) + 1;

                    _logger.LogInformation(
                        "✓ Notificación reenviada exitosamente - Notification {NotificationId}",
                        request.NotificationId);
                }

                _context.Notifications.Update(notification);
                await _context.SaveChangesAsync();

                return NotificationMapper.ToNotificationResponse(notification);
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reenviar Notification {NotificationId}", request.NotificationId);
                throw new RpcException(new Status(StatusCode.Internal,
                    $"Error interno: {ex.Message}"));
            }
        }

        /// <summary>
        /// Obtiene una razón de fallo aleatoria para emails
        /// </summary>
        private static string GetRandomEmailFailureReason()
        {
            var reasons = new[]
            {
                "Invalid email address",
                "Mailbox full",
                "Email server unreachable",
                "Recipient email blocked",
                "Spam filter rejection",
                "Email service rate limit exceeded"
            };

            return reasons[_random.Next(reasons.Length)];
        }

        /// <summary>
        /// Obtiene una razón de fallo aleatoria para SMS
        /// </summary>
        private static string GetRandomSMSFailureReason()
        {
            var reasons = new[]
            {
                "Invalid phone number",
                "Phone number not in service",
                "SMS gateway timeout",
                "Carrier rejection",
                "Daily SMS limit exceeded"
            };

            return reasons[_random.Next(reasons.Length)];
        }
    }
}
