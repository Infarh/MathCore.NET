using System.Linq;

namespace MathCore.NET.HTTP.Html
{
    public abstract class HElementBase
    {
        public static string SpacerPattern { get; set; } = "    ";
        protected static string GetSpacer(int level) => level <= 0 ? string.Empty : string.Concat(Enumerable.Repeat(SpacerPattern, level));

        public abstract string InnerText();

        public abstract string ToString(int level);

        /// <inheritdoc />
        public override string ToString() => ToString(0);


        public static implicit operator HElementBase(string text) => new Text(text);
    }
}