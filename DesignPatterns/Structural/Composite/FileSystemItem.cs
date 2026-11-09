namespace Structural.Composite;

/// <summary>
/// Component — the common abstraction for both leaves and composites.
/// Clients code against this type and never need to distinguish files from directories.
/// </summary>
public abstract class FileSystemItem(string name)
{
    public string Name { get; } = name;
    public abstract long GetSize();
    public abstract void Print(string indent = "");
}
