using System.Linq;

namespace MathCore.NET.HTTP.Html;

/// <summary>HTML-элемент menu (меню)</summary>
public class MenuList(params ListItem[] items) : TypedElement("menu", [.. items.Cast<HElementBase>()]);