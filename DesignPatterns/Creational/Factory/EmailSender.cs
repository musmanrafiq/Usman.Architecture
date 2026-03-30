namespace Creational.Factory;

public class EmailSender : INotificationSender
{
    public void Send(string recipient, string message)
        => Console.WriteLine($"[EMAIL] To: {recipient} | {message}");
}
