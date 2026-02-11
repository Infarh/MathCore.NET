using System.Collections;
using System.Linq;
using System.Text;

using MathCore.NET.Extensions;

namespace MathCore.NET.HTTP.Html;

/// <summary>HTML-элемент ul (ненумерованный список)</summary>
public class MarkedList : TypedElement
{
    /// <summary>Элементы для отображения в виде списка</summary>
    private IEnumerable Items { get; set; } = null!;

    /// <summary>Инициализирует новый экземпляр MarkedList с элементами ListItem</summary>
    /// <param name="items">Элементы списка</param>
    public MarkedList(params ListItem[] items) : base("ul", [.. items.Cast<HElementBase>()]) { }

    /// <summary>Инициализирует новый экземпляр MarkedList с произвольными элементами</summary>
    /// <param name="items">Элементы для отображения</param>
    public MarkedList(IEnumerable items) : base("ul") => Items = items;

    /// <summary>Добавить элементы к списку</summary>
    /// <param name="items">Элементы для добавления</param>
    public void Add(IEnumerable items) => Items = Items?.Concat(items) ?? items;

    /// <inheritdoc />
    public override string ToString(int level)
    {
        var spacer = GetSpacer(level);
        var result = new StringBuilder($"{spacer}<{Name}");
        if (HasAttributes) result.AppendFormat(" {0}", string.Join(" ", Attributes));

        result.AppendLine(">");
        var inner_html = InnerHtml(level);
        if (!string.IsNullOrEmpty(inner_html))
            result.AppendLine(inner_html);
        var items = Items;
        if (items != null)
            foreach (var item in items)
            {
                if (item is null) continue;
                var inner_text = item.ToString();
                var spacer2 = GetSpacer(level + 1);
                result.AppendLine(inner_text.Contains("\n")
                    ? $"{spacer2}<li>\r\n{spacer2}{spacer}{inner_text}\r\n{spacer2}</li>"
                    : $"{spacer2}<li>{inner_text}</li>");
            }
        result.AppendFormat("{1}</{0}>", Name, spacer);
        return result.ToString();
    }
}