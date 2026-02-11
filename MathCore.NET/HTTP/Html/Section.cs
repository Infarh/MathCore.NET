namespace MathCore.NET.HTTP.Html;

/// <summary>HTML-элемент section (раздел документа)</summary>
public class Section(params HElementBase[] elements) : TypedElement("section", elements);