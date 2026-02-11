namespace MathCore.NET.HTTP.Html;

/// <summary>HTML-элемент div (контейнер)</summary>
public class Div(params HElementBase[] elements) : TypedElement("div", elements);