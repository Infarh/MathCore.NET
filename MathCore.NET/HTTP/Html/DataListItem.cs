namespace MathCore.NET.HTTP.Html;

/// <summary>Элемент списка определений (dt и dd)</summary>
public class DataListItem(HElementBase dd, HElementBase dt) : TypedElement("dd")
{
    /// <summary>Определение (dd)</summary>
    public HElementBase DD { get; set; } = dd;

    /// <summary>Термин (dt)</summary>
    public HElementBase DT { get; set; } = dt;

    /// <inheritdoc />
    public override string ToString(int level)
    {
        var spacer = GetSpacer(level);
        return $"{spacer}<dd>\r\n{DD.ToString(level + 1)}\r\n{spacer}</dd>\r\n{spacer}<dt>\r\n{DT.ToString(level + 1)}\r\n{spacer}</dt>\r\n";
    }
}