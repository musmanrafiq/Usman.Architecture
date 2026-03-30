namespace Structural.Facade;

public class InventoryService
{
    public bool Reserve(string productId, int qty)
    {
        Console.WriteLine($"[Inventory] Reserved {qty}x {productId}");
        return true;
    }
}
