namespace MathCore.NET.HTTP.Html
{
    public class DataListItem : TypedElement
    {
        public HElementBase DD { get; set; }
        public HElementBase DT { get; set; }

        public DataListItem(HElementBase dd, HElementBase dt) : base("dd")
        {
            DD = dd;
            DT = dt;
        }

        /// <inheritdoc />
        public override string ToString(int level)
        {
            var spacer = GetSpacer(level);
            return $"{spacer}<dd>\r\n{DD.ToString(level+1)}\r\n{spacer}</dd>\r\n{spacer}<dt>\r\n{DT.ToString(level+1)}\r\n{spacer}</dt>\r\n";
        }
    }
}