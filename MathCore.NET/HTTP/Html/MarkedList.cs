using System.Collections;
using System.Linq;
using System.Text;
using MathCore.NET.Extensions;

namespace MathCore.NET.HTTP.Html
{
    public class MarkedList : TypedElement
    {
        private IEnumerable Items { get; set; }

        public MarkedList(params ListItem[] items) : base("ul", items.Cast<HElementBase>().ToArray()) { }

        public MarkedList(IEnumerable items) : base("ul") => Items = items;

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
}
