namespace MathCore.NET.HTTP.Html;

/// <summary>HTML-атрибут элемента</summary>
public class HAttribute(string AttributeName, string Value)
{
    /// <summary>Имя атрибута</summary>
    private string _AttributeName = AttributeName;

    /// <summary>Значение атрибута</summary>
    private string _Value = Value;

    /// <summary>Имя атрибута</summary>
    public string AttributeName { get => _AttributeName; set => _AttributeName = value; }

    /// <summary>Значение атрибута</summary>
    public string Value { get => _Value; set => _Value = value; }

    /// <inheritdoc />
    public override string ToString() => $"{_AttributeName}=\"{_Value}\"";
}

/// <summary>Атрибут класса HTML-элемента</summary>
public class ClassAttribute(string Name) : HAttribute("class", Name);

/// <summary>Атрибут идентификатора HTML-элемента</summary>
public class IdAttribute(string Name) : HAttribute("id", Name);