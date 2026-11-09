namespace Structural.Composite;

public static class CompositeDemo
{
    public static void Run()
    {
        var root = new Directory("root");
        var src = new Directory("src");
        var tests = new Directory("tests");

        src.Add(new File("Program.cs", 5_120));
        src.Add(new File("OrderService.cs", 12_288));
        tests.Add(new File("OrderTests.cs", 8_192));

        root.Add(src);
        root.Add(tests);
        root.Add(new File("README.md", 2_048));

        root.Print();
        Console.WriteLine($"Total size: {root.GetSize() / 1024.0:F1} KB");
    }
}
