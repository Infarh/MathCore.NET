using System.Linq;

namespace MathCore.NET.HTTP.Html;

public class DataList(params DataListItem[] items) : TypedElement("dd", [.. items.Cast<HElementBase>()]);