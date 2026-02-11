namespace MathCore.NET.HTTP.Html;

/// <summary>HTML-элемент head (заголовок документа)</summary>
public class Head : TypedElement { 
    /// <summary>Инициализирует новый экземпляр Head</summary>
    /// <param name="elements">Вложенные элементы</param>
    public Head(params HElementBase[] elements) : base("head", elements) => AlwaysOpen = true; 
}