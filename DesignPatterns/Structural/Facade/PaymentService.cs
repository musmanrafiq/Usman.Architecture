namespace Structural.Facade;

public class PaymentService
{
    public bool Charge(string customerId, decimal total)
    {
        Console.WriteLine($"[Payment] Charged {total:C} to customer {customerId}");
        return true;
    }
}
