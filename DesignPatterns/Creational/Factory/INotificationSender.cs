namespace Creational.Factory;

public interface INotificationSender
{
    void Send(string recipient, string message);
}

public enum NotificationChannel { Email, Sms, Push }
