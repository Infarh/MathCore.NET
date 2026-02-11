namespace MathCore.NET.HTTP.Html;

public class DataListItem(HElementBase dd, HElementBase dt) : TypedElement("dd")
{
    public HElementBase DD { get; set; } = dd;

    public HElementBase DT { get; set; } = dt;

    /// <inheritdoc />
    public override string ToString(int level)
    {
        var spacer = GetSpacer(level);
        return $"{spacer}<dd>\r\n{DD.ToString(level + 1)}\r\n{spacer}</dd>\r\n{spacer}<dt>\r\n{DT.ToString(level + 1)}\r\n{spacer}</dt>\r\n";
    }
}