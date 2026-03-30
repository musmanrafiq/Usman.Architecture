namespace Behavioral.Command;

public static class CommandDemo
{
    public static void Run()
    {
        var cart = new ShoppingCart();
        var invoker = new CartCommandInvoker();

        invoker.Execute(new AddItemCommand(cart, "Laptop", 999.99m));
        invoker.Execute(new AddItemCommand(cart, "Mouse", 29.99m));
        invoker.Execute(new AddItemCommand(cart, "USB Hub", 19.99m));

        cart.PrintCart();

        invoker.UndoLast(); // removes USB Hub
        cart.PrintCart();
    }
}
