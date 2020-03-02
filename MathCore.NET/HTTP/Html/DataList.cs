using System.Linq;

namespace MathCore.NET.HTTP.Html
{
    public class DataList : TypedElement
    {
        public DataList(params DataListItem[] items) : base("dd", items.Cast<HElementBase>().ToArray()) { }
    }
}