using System;
using System.Collections.Generic;
using System.Linq;

namespace MathCore.NET.HTTP.Html
{
    public class Link : TypedElement
    {
        /// <inheritdoc />
        public override bool OnlyOpen { get => true; set => throw new NotSupportedException(); }

        public string Relation
        {
            get => Attributes.FirstOrDefault(a => a.AttributeName.Equals("rel", StringComparison.InvariantCultureIgnoreCase))?.Value;
            set
            {
                var attribute = Attributes.FirstOrDefault(a => a.AttributeName.Equals("rel", StringComparison.InvariantCultureIgnoreCase));
                if (attribute != null) attribute.Value = value;
                else Attributes.Add(new HAttribute("rel", value));
            }
        }

        public string Reference
        {
            get => Attributes.FirstOrDefault(a => a.AttributeName.Equals("href", StringComparison.InvariantCultureIgnoreCase))?.Value;
            set
            {
                var attribute = Attributes.FirstOrDefault(a => a.AttributeName.Equals("href", StringComparison.InvariantCultureIgnoreCase));
                if (attribute != null) attribute.Value = value;
                else Attributes.Add(new HAttribute("href", value));
            }
        }

        public Link() : base("link") { }

        public Link(string relation, string href) : base("link")
        {
            List<HAttribute> attributes = null;
            if (!string.IsNullOrWhiteSpace(relation)) (attributes = Attributes).Add(new HAttribute("rel", relation));
            if (!string.IsNullOrWhiteSpace(href)) (attributes ?? Attributes).Add(new HAttribute("href", href));
        }
    }
}