namespace Creational.Factory;

public class SmsSender : INotificationSender
{
    public void Send(string recipient, string message)
        => Console.WriteLine($"[SMS]   To: {recipient} | {message}");
}
