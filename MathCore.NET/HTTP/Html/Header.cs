namespace MathCore.NET.HTTP.Html;

/// <summary>HTML-элемент header (заголовок раздела)</summary>
public class Header(params HElementBase[] elements) : TypedElement("header", elements);