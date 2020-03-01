using System;
using System.Linq;

namespace MathCore.NET.HTTP.Html
{
    public class Script : TypedElement
    {
        /// <inheritdoc />
        public override bool AlwaysOpen { get => true; set => throw new NotSupportedException(); }

        public string Source
        {
            get => Attributes.FirstOrDefault(a => a.AttributeName.Equals("src", StringComparison.InvariantCultureIgnoreCase))?.Value;
            set
            {
                var attribute = Attributes.FirstOrDefault(a => a.AttributeName.Equals("src", StringComparison.InvariantCultureIgnoreCase));
                if (attribute != null) attribute.Value = value;
                else Attributes.Add(new HAttribute("src", value));
            }
        }

        public Script() : base("script") { }
        public Script(string script) : base("script", new Text(script)) { }
    }
}