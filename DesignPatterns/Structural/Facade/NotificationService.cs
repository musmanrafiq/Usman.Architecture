namespace Structural.Facade;

public class NotificationService
{
    public void Notify(string customerId, string message)
        => Console.WriteLine($"[Notification] -> customer {customerId}: {message}");
}
