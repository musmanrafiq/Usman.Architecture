namespace Behavioral.Strategy;

public static class StrategyDemo
{
    public static void Run()
    {
        var calc = new ShippingCalculator(new StandardShipping());
        calc.Calculate(2.5m, "London");

        calc.SetStrategy(new ExpressShipping());
        calc.Calculate(2.5m, "London");

        calc.SetStrategy(new InternationalShipping());
        calc.Calculate(2.5m, "Tokyo");
    }
}
