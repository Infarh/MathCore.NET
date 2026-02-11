using System.Linq;

namespace MathCore.NET.HTTP.Html;

/// <summary>HTML-элемент li (элемент списка)</summary>
public class ListItem(params HElementBase[] elements) : TypedElement("li", elements);