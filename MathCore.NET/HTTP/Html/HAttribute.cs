namespace MathCore.NET.HTTP.Html;

public class HAttribute(string AttributeName, string Value)
{
    private string _AttributeName = AttributeName;
    private string _Value = Value;

    public string AttributeName { get => _AttributeName; set => _AttributeName = value; }
    public string Value { get => _Value; set => _Value = value; }

    /// <inheritdoc />
    public override string ToString() => $"{_AttributeName}=\"{_Value}\"";
}

public class ClassAttribute(string Name) : HAttribute("class", Name);

public class IdAttribute(string Name) : HAttribute("id", Name);