namespace MathCore.NET.HTTP.Html;

/// <summary>HTML-элемент для обычного текста</summary>
public class Text(string text) : HElementBase
{
    /// <summary>Текстовое содержимое</summary>
    public string Value { get; set; } = text;

    /// <inheritdoc />
    public override string InnerText() => Value;

    /// <inheritdoc />
    public override string ToString(int level) => $"{GetSpacer(level)}{Value}";
}