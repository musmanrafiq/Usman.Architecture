namespace Scenarios.Notifications;

public record NotificationEvent(
    string UserId,
    string Subject,
    string Body,
    NotificationPriority Priority);
