2. NotificationService.gRPC - Puerto 7005
Base de datos: NotificationDb
Funcionalidad:

Envío de emails (mock, no envío real)
Envío de SMS (mock, no envío real)
Historial de notificaciones por usuario
Reenvío de notificaciones fallidas
Mock: 5% de emails y 3% de SMS fallan para testing

Métodos gRPC:

SendEmail: Enviar notificación por email
SendSMS: Enviar notificación por SMS
GetNotificationHistory: Historial con paginación
GetNotification: Consultar notificación específica
ResendNotification: Reintentar envío fallido

Templates disponibles:

OrderConfirmation, PaymentSuccess, PaymentFailed
ShippingUpdate, DeliveryConfirmation, OrderCancelled

Datos de prueba:

9 notificaciones pre-cargadas (emails y SMS de diferentes órdenes)

# Enviar email
grpcurl -plaintext -d '{
  "user_id": 2,
  "email_to": "test@email.com",
  "subject": "Test",
  "body": "Test message",
  "template": "OrderConfirmation"
}' localhost:7005 notificationservice.NotificationService/SendEmail

# Consultar historial
grpcurl -plaintext -d '{"user_id": 2}' localhost:7005 notificationservice.NotificationService/GetNotificationHistory

-------------
Metodos de NotificationService.gRPC:

SendEmail(userId, orderId, template) → Success/Failure
SendSMS(phone, message) → Success/Failure (mock)
GetNotificationHistory(userId) → List de notificaciones enviadas

-------------
Health Checks
NotificationService: http://localhost:7005/health
