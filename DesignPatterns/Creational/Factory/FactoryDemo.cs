namespace Creational.Factory;

public static class FactoryDemo
{
    public static void Run()
    {
        var channels = new[] { NotificationChannel.Email, NotificationChannel.Sms, NotificationChannel.Push };

        foreach (var channel in channels)
        {
            var sender = NotificationSenderFactory.Create(channel);
            sender.Send("user@example.com", "Your order has shipped.");
        }
    }
}
