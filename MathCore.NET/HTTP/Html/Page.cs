using System;
using System.Linq;

namespace MathCore.NET.HTTP.Html
{
    public class Page
    {
        private Head _Head = new Head { AlwaysOpen = true };
        private Body _Body = new Body { AlwaysOpen = true };

        public Head Head { get => _Head; set => _Head = value ?? new Head(); }

        public Body Body { get => _Body; set => _Body = value ?? new Body(); }

        public string Title
        {
            get => _Head.Elements.OfType<HElement>().FirstOrDefault(e => e.Name == "title")?.InnerText();
            set
            {
                var title = _Head.Elements.OfType<Title>().FirstOrDefault();
                if (title is null)
                {
                    _Head.Elements.Add(new Title(new Text(value)));
                    return;
                }
                var title_elements = title.Elements;
                title_elements.Clear();
                title_elements.Add(new Text(value));
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var str = $"<!DOCTYPE html>\r\n{new HElement("html", _Head, _Body)}";
            Console.WriteLine(str);
            return str;
        }
    }
}
