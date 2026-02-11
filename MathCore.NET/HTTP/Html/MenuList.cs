using System.Linq;

namespace MathCore.NET.HTTP.Html;

public class MenuList(params ListItem[] items) : TypedElement("menu", [.. items.Cast<HElementBase>()]);