using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathCore.NET.HTTP.Html
{
    public class HElement : HElementBase, IEnumerable<HElementBase>, IEnumerable<HAttribute>
    {
        private string _Name;
        private List<HAttribute> _Attributes;
        private List<HElementBase> _Elements;
        private bool _AlwaysOpen;
        private bool _OnlyOpen;

        public virtual string Name { get => _Name; set => _Name = value; }

        public List<HElementBase> Elements { get => _Elements ??= new List<HElementBase>(); set => _Elements = value; }

        public bool HasElements => _Elements != null && _Elements.Count > 0;

        public List<HAttribute> Attributes { get => _Attributes ??= new List<HAttribute>(); set => _Attributes = value; }
        public bool HasAttributes => _Attributes != null && _Attributes.Count > 0;

        public virtual bool AlwaysOpen { get => _AlwaysOpen; set => _AlwaysOpen = value; }
        public virtual bool OnlyOpen { get => _OnlyOpen; set => _OnlyOpen = value; }

        public HElement(string Name, params HElementBase[] elements)
        {
            _Name = Name;
            if (elements.Length > 0) _Elements = elements.ToList();
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
            ? string.Join("\r\n", _Elements.Select(e => e.ToString(level + 1)))
            : string.Empty;

        public override string InnerText() => InnerText(0);
        protected string InnerText(int level) => HasElements
            ? string.Join($"{GetSpacer(level + 1)}\r\n", _Elements.Select(e => e.InnerText()))
            : string.Empty;

        /// <inheritdoc />
        public override string ToString(int level)
        {
            var spacer = GetSpacer(level);
            var result = new StringBuilder($"{spacer}<{_Name}");
            if (HasAttributes) result.AppendFormat(" {0}", string.Join(" ", _Attributes));

            if (!HasElements)
                return OnlyOpen
                    ? $"{result}>"
                    : AlwaysOpen
                        ? $"{result}></{_Name}>"
                        : $"{result}/>";

            if (_Elements.Count == 1)
            {
                var inner_text = InnerHtml(level);
                if (!inner_text.Contains("\r\n"))
                    return $"{result}>{inner_text.Trim()}</{_Name}>";
            }

            result.AppendLine(">");
            result.AppendLine(InnerHtml(level));
            result.AppendFormat("{1}</{0}>", _Name, spacer);

            return result.ToString();
        }

        /// <inheritdoc />
        public IEnumerator<HElementBase> GetEnumerator() => _Elements.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_Elements).GetEnumerator();

        /// <inheritdoc />
        IEnumerator<HAttribute> IEnumerable<HAttribute>.GetEnumerator() => _Attributes.GetEnumerator();
    }
}