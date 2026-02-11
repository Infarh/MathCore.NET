namespace MathCore.NET.HTTP.Html;

/// <summary>HTML-элемент body (тело документа)</summary>
public class Body : TypedElement { 
    /// <summary>Инициализирует новый экземпляр Body</summary>
    /// <param name="elements">Вложенные элементы</param>
    public Body(params HElementBase[] elements) : base("body", elements) => AlwaysOpen = true; 
}