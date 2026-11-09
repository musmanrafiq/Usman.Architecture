namespace Behavioral.Command;

/// <summary>Receiver — the object that knows how to perform the actual work.</summary>
public class ShoppingCart
{
    private readonly List<(string Item, decimal Price)> _items = [];

    public void AddItem(string item, decimal price)
    {
        _items.Add((item, price));
        Console.WriteLine($"[Cart] Added: {item} ({price:C})");
    }

    public void RemoveItem(string item)
    {
        var found = _items.FirstOrDefault(i => i.Item == item);
        if (found != default)
        {
            _items.Remove(found);
            Console.WriteLine($"[Cart] Removed: {item}");
        }
    }

    public void PrintCart()
    {
        Console.WriteLine(
            $"[Cart] Items: {string.Join(", ", _items.Select(i => i.Item))} | " +
            $"Total: {_items.Sum(i => i.Price):C}");
    }
}
