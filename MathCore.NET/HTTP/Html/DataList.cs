using System.Linq;

namespace MathCore.NET.HTTP.Html;

/// <summary>HTML-элемент dd (список определений)</summary>
public class DataList(params DataListItem[] items) : TypedElement("dd", [.. items.Cast<HElementBase>()]);