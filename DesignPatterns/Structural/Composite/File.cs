namespace Structural.Composite;

/// <summary>Leaf — has no children, just a fixed size.</summary>
public class File(string name, long sizeBytes) : FileSystemItem(name)
{
    public override long GetSize() => sizeBytes;

    public override void Print(string indent = "")
        => Console.WriteLine($"{indent}├── {Name} ({sizeBytes / 1024.0:F1} KB)");
}
