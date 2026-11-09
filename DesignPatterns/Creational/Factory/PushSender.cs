namespace Creational.Factory;

public class PushSender : INotificationSender
{
    public void Send(string recipient, string message)
        => Console.WriteLine($"[PUSH]  To: {recipient} | {message}");
}
