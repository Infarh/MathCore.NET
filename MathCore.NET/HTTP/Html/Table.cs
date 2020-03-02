using System;
using System.Linq;

namespace MathCore.NET.HTTP.Html
{
    public class Table : TypedElement
    {
        private const StringComparison __StringComparison = StringComparison.InvariantCultureIgnoreCase;
        public TableHeader Header
        {
            get
            {
                if (!HasElements) return null;
                var elements = Elements;
                var header_element = elements
                    .OfType<HElement>()
                    .FirstOrDefault(e => e.Name?.Equals("thead", __StringComparison) ?? false);
                if (header_element is null) return null;
                if (header_element is TableHeader header) return header;
                header = new TableHeader(header_element.ToArray<HElementBase>());
                var header_index = elements.IndexOf(header_element);
                elements.Remove(header_element);
                elements.Insert(header_index, header);
                return header;
            }
            set
            {
                if (value is null)
                {
                    if (HasElements) Elements.RemoveAll(e => (e as HElement)?.Name?.Equals("theader", __StringComparison) ?? false);
                    return;
                }

                var elements = Elements;
                if (elements.Count == 0)
                {
                    elements.Add(value);
                    return;
                }
                if (elements.Contains(value)) return;
                Elements.RemoveAll(e => (e as HElement)?.Name?.Equals("theader", __StringComparison) ?? false);
                elements.Add(value);
            }
        }

        public TableBody Body
        {
            get
            {
                if (!HasElements) return null;
                var elements = Elements;
                var header_element = elements
                    .OfType<HElement>()
                    .FirstOrDefault(e => e.Name?.Equals("tbody", __StringComparison) ?? false);
                if (header_element is null) return null;
                if (header_element is TableBody header) return header;
                header = new TableBody(header_element.ToArray<HElementBase>());
                var header_index = elements.IndexOf(header_element);
                elements.Remove(header_element);
                elements.Insert(header_index, header);
                return header;
            }
            set
            {
                if (value is null)
                {
                    if (HasElements) Elements.RemoveAll(e => (e as HElement)?.Name?.Equals("tbody", __StringComparison) ?? false);
                    return;
                }

                var elements = Elements;
                if (elements.Count == 0)
                {
                    elements.Add(value);
                    return;
                }
                if (elements.Contains(value)) return;
                Elements.RemoveAll(e => (e as HElement)?.Name?.Equals("tbody", __StringComparison) ?? false);
                elements.Add(value);
            }
        }

        public Table(params HElementBase[] elements) : base("table", elements) { }
    }

    public class TableHeader : TypedElement { public TableHeader(params HElementBase[] elements) : base("thead", elements) { } }

    public class TableBody : TypedElement { public TableBody(params HElementBase[] elements) : base("tbody", elements) { } }
}