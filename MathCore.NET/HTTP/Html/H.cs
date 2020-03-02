namespace MathCore.NET.HTTP.Html
{
    public class H : TypedElement { public H(int index, params HElementBase[] elements) : base($"h{index}", elements) { } }

    public class H1 : H { public H1(params HElementBase[] elements) : base(1, elements) { } }

    public class H2 : H { public H2(params HElementBase[] elements) : base(2, elements) { } }

    public class H3 : H { public H3(params HElementBase[] elements) : base(3, elements) { } }

    public class H4 : H { public H4(params HElementBase[] elements) : base(4, elements) { } }
}