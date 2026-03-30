namespace Creational.AbstractFactory;

public static class AbstractFactoryDemo
{
    public static void Run()
    {
        Console.WriteLine("--- Windows UI ---");
        new UiRenderer(new WindowsUiFactory()).RenderForm();

        Console.WriteLine("--- Mac UI ---");
        new UiRenderer(new MacUiFactory()).RenderForm();
    }
}
