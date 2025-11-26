using FluentValidation;
using ECommerceGRPC.NotificationService;
using System.Text.RegularExpressions;

namespace NotificationService.gRPC.Validators
{
    /// <summary>
    /// Validador para SendEmailRequest
    /// </summary>
    public class SendEmailRequestValidator : AbstractValidator<SendEmailRequest>
    {
        public SendEmailRequestValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0)
                .WithMessage("User ID debe ser mayor a 0");

            RuleFor(x => x.EmailTo)
                .NotEmpty()
                .WithMessage("Email To es requerido")
                .EmailAddress()
                .WithMessage("Email To debe ser una dirección de email válida")
                .MaximumLength(255)
                .WithMessage("Email To no puede exceder 255 caracteres");

            RuleFor(x => x.Subject)
                .NotEmpty()
                .WithMessage("Subject es requerido")
                .MaximumLength(255)
                .WithMessage("Subject no puede exceder 255 caracteres");

            RuleFor(x => x.Body)
                .NotEmpty()
                .WithMessage("Body es requerido")
                .MaximumLength(10000)
                .WithMessage("Body no puede exceder 10000 caracteres");

            RuleFor(x => x.Template)
                .NotEmpty()
                .WithMessage("Template es requerido")
                .MaximumLength(100)
                .WithMessage("Template no puede exceder 100 caracteres");
        }
    }

    /// <summary>
    /// Validador para SendSMSRequest
    /// </summary>
    public class SendSMSRequestValidator : AbstractValidator<SendSMSRequest>
    {
        public SendSMSRequestValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0)
                .WithMessage("User ID debe ser mayor a 0");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .WithMessage("Phone Number es requerido")
                .Must(BeValidPhoneNumber)
                .WithMessage("Phone Number debe ser un número de teléfono válido (formato: +506XXXXXXXX)")
                .MaximumLength(20)
                .WithMessage("Phone Number no puede exceder 20 caracteres");

            RuleFor(x => x.Message)
                .NotEmpty()
                .WithMessage("Message es requerido")
                .MaximumLength(160)
                .WithMessage("SMS Message no puede exceder 160 caracteres (límite estándar SMS)");

            RuleFor(x => x.Template)
                .NotEmpty()
                .WithMessage("Template es requerido")
                .MaximumLength(100)
                .WithMessage("Template no puede exceder 100 caracteres");
        }

        private bool BeValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
                return false;

            // Formato internacional: +[código país][número]
            // Ejemplos: +50612345678, +15551234567
            var phoneRegex = new Regex(@"^\+\d{10,15}$");
            return phoneRegex.IsMatch(phoneNumber);
        }
    }

    /// <summary>
    /// Validador para GetNotificationHistoryRequest
    /// </summary>
    public class GetNotificationHistoryRequestValidator : AbstractValidator<GetNotificationHistoryRequest>
    {
        public GetNotificationHistoryRequestValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0)
                .WithMessage("User ID debe ser mayor a 0");

            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .When(x => x.PageNumber > 0)
                .WithMessage("Page Number debe ser mayor a 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .When(x => x.PageSize > 0)
                .WithMessage("Page Size debe ser mayor a 0")
                .LessThanOrEqualTo(100)
                .When(x => x.PageSize > 0)
                .WithMessage("Page Size no puede exceder 100");

            RuleFor(x => x.NotificationType)
                .Must(BeValidNotificationType)
                .When(x => !string.IsNullOrEmpty(x.NotificationType))
                .WithMessage("Notification Type debe ser: Email, SMS, o vacío para todos");
        }

        private bool BeValidNotificationType(string notificationType)
        {
            if (string.IsNullOrEmpty(notificationType))
                return true;

            var validTypes = new[] { "Email", "SMS", "Push" };
            return validTypes.Contains(notificationType);
        }
    }

    /// <summary>
    /// Validador para GetNotificationRequest
    /// </summary>
    public class GetNotificationRequestValidator : AbstractValidator<GetNotificationRequest>
    {
        public GetNotificationRequestValidator()
        {
            RuleFor(x => x.NotificationId)
                .GreaterThan(0)
                .WithMessage("Notification ID debe ser mayor a 0");
        }
    }

    /// <summary>
    /// Validador para ResendNotificationRequest
    /// </summary>
    public class ResendNotificationRequestValidator : AbstractValidator<ResendNotificationRequest>
    {
        public ResendNotificationRequestValidator()
        {
            RuleFor(x => x.NotificationId)
                .GreaterThan(0)
                .WithMessage("Notification ID debe ser mayor a 0");
        }
    }
}
