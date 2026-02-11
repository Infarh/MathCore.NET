namespace MathCore.NET.HTTP.Html;

public class Text(string text) : HElementBase
{
    public string Value { get; set; } = text;

    /// <inheritdoc />
    public override string InnerText() => Value;

    /// <inheritdoc />
    public override string ToString(int level) => $"{GetSpacer(level)}{Value}";
}