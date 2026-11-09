namespace Creational.AbstractFactory;

public class UiRenderer(IUiFactory factory)
{
    public void RenderForm()
    {
        Console.WriteLine(factory.CreateButton().Render());
        Console.WriteLine(factory.CreateCheckbox().Render());
    }
}
