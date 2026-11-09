namespace Creational.Factory;

public static class NotificationSenderFactory
{
    public static INotificationSender Create(NotificationChannel channel) => channel switch
    {
        NotificationChannel.Email => new EmailSender(),
        NotificationChannel.Sms => new SmsSender(),
        NotificationChannel.Push => new PushSender(),
        _ => throw new ArgumentOutOfRangeException(nameof(channel))
    };
}
