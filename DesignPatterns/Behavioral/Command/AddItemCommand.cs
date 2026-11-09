namespace Behavioral.Command;

public class AddItemCommand(ShoppingCart cart, string item, decimal price) : ICommand
{
    public void Execute() => cart.AddItem(item, price);
    public void Undo() => cart.RemoveItem(item);
}
