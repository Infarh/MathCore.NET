using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathCore.NET.HTTP.Html;

public class HElement : HElementBase, IEnumerable<HElementBase>, IEnumerable<HAttribute>
{
    public virtual string Name { get; set; }

    public List<HElementBase> Elements { get; set; } = new();

    public bool HasElements => Elements.Count > 0;

    public List<HAttribute> Attributes { get; set; } = new();

    public bool HasAttributes => Attributes.Count > 0;

    public virtual bool AlwaysOpen { get; set; }

    public virtual bool OnlyOpen { get; set; }

    public HElement(string Name, params HElementBase[] elements)
    {
        this.Name = Name;
        if (elements.Length > 0) Elements = elements.ToList();
    }

    public void Add(params HAttribute[] attribute) => Attributes.AddRange(attribute);

    public void Add(params HElementBase[] element) => Elements.AddRange(element);

    public void Add(params object[] items)
    {
        foreach (var item in items)
        {
            switch (item)
            {
                case HAttribute attribute:
                    Attributes.Add(attribute);
                    break;
                case HElement element:
                    Elements.Add(element);
                    break;
                case string str:
                    Elements.Add(new Text(str));
                    break;
                default:
                    Elements.Add(new Text(item.ToString()));
                    break;
            }
        }
    }

    public string InnerHtml() => InnerHtml(0);
    protected string InnerHtml(int level) => HasElements
        ? string.Join("\r\n", Elements.Select(e => e.ToString(level + 1)))
        : string.Empty;

    public override string InnerText() => InnerText(0);
    protected string InnerText(int level) => HasElements
        ? string.Join($"{GetSpacer(level + 1)}\r\n", Elements.Select(e => e.InnerText()))
        : string.Empty;

    /// <inheritdoc />
    public override string ToString(int level)
    {
        var spacer = GetSpacer(level);
        var result = new StringBuilder($"{spacer}<{Name}");
        if (HasAttributes) result.AppendFormat(" {0}", string.Join(" ", Attributes));

        if (!HasElements)
            return OnlyOpen
                ? $"{result}>"
                : AlwaysOpen
                    ? $"{result}></{Name}>"
                    : $"{result}/>";

        if (Elements.Count == 1)
        {
            var inner_text = InnerHtml(level);
            if (!inner_text.Contains("\r\n"))
                return $"{result}>{inner_text.Trim()}</{Name}>";
        }

        result.AppendLine(">");
        result.AppendLine(InnerHtml(level));
        result.AppendFormat("{1}</{0}>", Name, spacer);

        return result.ToString();
    }

    /// <inheritdoc />
    public IEnumerator<HElementBase> GetEnumerator() => Elements.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Elements).GetEnumerator();

    /// <inheritdoc />
    IEnumerator<HAttribute> IEnumerable<HAttribute>.GetEnumerator() => Attributes.GetEnumerator();
}