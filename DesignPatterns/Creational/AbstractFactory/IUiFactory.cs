namespace Creational.AbstractFactory;

public interface IUiFactory
{
    IButton CreateButton();
    ICheckbox CreateCheckbox();
}
