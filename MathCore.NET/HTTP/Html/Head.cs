namespace MathCore.NET.HTTP.Html
{
    public class Head : TypedElement { public Head(params HElementBase[] elements) : base("head", elements) => AlwaysOpen = true; }
}