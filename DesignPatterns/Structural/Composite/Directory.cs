namespace Structural.Composite;

/// <summary>
/// Composite — contains children that can themselves be files or directories.
/// GetSize() and Print() recurse through the whole tree uniformly.
/// </summary>
public class Directory(string name) : FileSystemItem(name)
{
    private readonly List<FileSystemItem> _children = [];

    public void Add(FileSystemItem item) => _children.Add(item);
    public void Remove(FileSystemItem item) => _children.Remove(item);

    public override long GetSize() => _children.Sum(c => c.GetSize());

    public override void Print(string indent = "")
    {
        Console.WriteLine($"{indent}└── {Name}/ ({GetSize() / 1024.0:F1} KB)");
        foreach (var child in _children)
            child.Print(indent + "    ");
    }
}
