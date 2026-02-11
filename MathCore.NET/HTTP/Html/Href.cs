using System.Linq;

namespace MathCore.NET.HTTP.Html;

/// <summary>HTML-элемент a (гиперссылка)</summary>
public class Href : TypedElement
{
    /// <summary>URL ссылки</summary>
    public string Link
    {
        get => Attributes.FirstOrDefault(a => a.AttributeName == "href")?.Value ?? "";
        set
        {
            var href = Attributes.FirstOrDefault(a => a.AttributeName == "href");
            if (href is null) Attributes.Add(new("href", value)); else href.Value = value;
        }
    }

    /// <summary>Инициализирует новый экземпляр Href без указания ссылки</summary>
    /// <param name="elements">Вложенные элементы</param>
    public Href(params HElementBase[] elements) : base("a", elements) => Attributes.Add(new("href", ""));

    /// <summary>Инициализирует новый экземпляр Href с указанием ссылки</summary>
    /// <param name="link">URL ссылки</param>
    /// <param name="elements">Вложенные элементы</param>
    public Href(string link, params HElementBase[] elements) : base("a", elements) => Attributes.Add(new("href", link));
}