namespace Creational.Prototype;

public class ReportTemplate : DocumentTemplate
{
    public string Department { get; set; } = string.Empty;

    public override DocumentTemplate Clone() =>
        new ReportTemplate
        {
            Title = Title,
            Content = Content,
            Tags = [.. Tags],   // deep copy — independent list
            Department = Department
        };
}
