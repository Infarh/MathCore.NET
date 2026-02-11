namespace MathCore.NET.HTTP.Html;

public class H(int index, params HElementBase[] elements) : TypedElement($"h{index}", elements);

public class H1(params HElementBase[] elements) : H(1, elements);

public class H2(params HElementBase[] elements) : H(2, elements);

public class H3(params HElementBase[] elements) : H(3, elements);

public class H4(params HElementBase[] elements) : H(4, elements);