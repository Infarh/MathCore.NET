namespace MathCore.NET.HTTP.Html
{
    public class Body : TypedElement { public Body(params HElementBase[] elements) : base("body", elements) => AlwaysOpen = true; }
}