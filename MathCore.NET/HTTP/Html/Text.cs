namespace MathCore.NET.HTTP.Html
{
    public class Text : HElementBase
    {
        public string Value { get; set; }

        public Text(string text) => Value = text;

        /// <inheritdoc />
        public override string InnerText() => Value;

        /// <inheritdoc />
        public override string ToString(int level) => $"{GetSpacer(level)}{Value}";
    }
}