namespace MathCore.NET.HTTP.Html;

/// <summary>HTML-элемент h (заголовок уровня)</summary>
/// <param name="index">Уровень заголовка (1-6)</param>
/// <param name="elements">Вложенные элементы</param>
public class H(int index, params HElementBase[] elements) : TypedElement($"h{index}", elements);

/// <summary>HTML-элемент h1 (заголовок 1-го уровня)</summary>
/// <param name="elements">Вложенные элементы</param>
public class H1(params HElementBase[] elements) : H(1, elements);

/// <summary>HTML-элемент h2 (заголовок 2-го уровня)</summary>
/// <param name="elements">Вложенные элементы</param>
public class H2(params HElementBase[] elements) : H(2, elements);

/// <summary>HTML-элемент h3 (заголовок 3-го уровня)</summary>
/// <param name="elements">Вложенные элементы</param>
public class H3(params HElementBase[] elements) : H(3, elements);

/// <summary>HTML-элемент h4 (заголовок 4-го уровня)</summary>
/// <param name="elements">Вложенные элементы</param>
public class H4(params HElementBase[] elements) : H(4, elements);