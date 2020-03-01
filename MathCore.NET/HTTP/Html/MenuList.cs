using System.Linq;

namespace MathCore.NET.HTTP.Html
{
    public class MenuList : TypedElement
    {
        public MenuList(params ListItem[] items) : base("menu", items.Cast<HElementBase>().ToArray()) { }
    }
}