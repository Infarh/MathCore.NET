using System.Linq;

namespace MathCore.NET.HTTP.Html
{
    public class Href : TypedElement
    {
        public string Link
        {
            get => Attributes.FirstOrDefault(a => a.AttributeName == "href")?.Value;
            set
            {
                var href = Attributes.FirstOrDefault(a => a.AttributeName == "href");
                if (href is null) Attributes.Add(new HAttribute("href", value)); else href.Value = value;
            }
        }
        public Href(params HElementBase[] elements) : base("a", elements) => Attributes.Add(new HAttribute("href", ""));

        public Href(string link, params HElementBase[] elements) : base("a", elements) => Attributes.Add(new HAttribute("href", link));
    }
}