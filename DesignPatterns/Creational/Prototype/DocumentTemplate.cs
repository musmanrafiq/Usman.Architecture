namespace Creational.Prototype;

/// <summary>
/// Base class for all document templates.
/// Declares Clone() so each subclass performs a deep copy of its own state.
/// </summary>
public abstract class DocumentTemplate
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];

    public abstract DocumentTemplate Clone();

    public override string ToString() =>
        $"[{GetType().Name}] Title='{Title}', Tags=[{string.Join(", ", Tags)}]";
}
