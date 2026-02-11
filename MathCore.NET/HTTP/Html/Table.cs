using System;
using System.Linq;

namespace MathCore.NET.HTTP.Html;

/// <summary>HTML-элемент table (таблица)</summary>
public class Table(params HElementBase[] elements) : TypedElement("table", elements)
{
    /// <summary>Игнорирование регистра при сравнении строк</summary>
    private const StringComparison __StringComparison = StringComparison.InvariantCultureIgnoreCase;

    /// <summary>Получить или установить заголовок таблицы (thead)</summary>
    public TableHeader? Header
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
            header = new([.. header_element]);
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

    /// <summary>Получить или установить тело таблицы (tbody)</summary>
    public TableBody? Body
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
            header = new([.. header_element]);
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
}

/// <summary>HTML-элемент thead (заголовок таблицы)</summary>
public class TableHeader(params HElementBase[] elements) : TypedElement("thead", elements);

/// <summary>HTML-элемент tbody (тело таблицы)</summary>
public class TableBody(params HElementBase[] elements) : TypedElement("tbody", elements);