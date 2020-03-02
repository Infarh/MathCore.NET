namespace MathCore.NET.HTTP.Html
{
    public class HAttribute
    {
        private string _AttributeName;
        private string _Value;

        public string AttributeName { get => _AttributeName; set => _AttributeName = value; }
        public string Value { get => _Value; set => _Value = value; }

        public HAttribute(string AttributeName, string Value)
        {
            _AttributeName = AttributeName;
            _Value = Value;
        }

        /// <inheritdoc />
        public override string ToString() => $"{_AttributeName}=\"{_Value}\"";
    }

    public class ClassAttribute : HAttribute { public ClassAttribute(string Name) : base("class", Name) { } }
    public class IdAttribute : HAttribute { public IdAttribute(string Name) : base("id", Name) { } }
}