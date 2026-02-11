using System;
using System.Linq;

namespace MathCore.NET.HTTP.Html;

/// <summary>HTML-страница с head и body элементами</summary>
public class Page
{
    /// <summary>Заголовок (head) страницы</summary>
    private Head _Head = new() { AlwaysOpen = true };

    /// <summary>Тело (body) страницы</summary>
    private Body _Body = new() { AlwaysOpen = true };

    /// <summary>Получить или установить заголовок (head) страницы</summary>
    public Head Head { get => _Head; set => _Head = value ?? []; }

    /// <summary>Получить или установить тело (body) страницы</summary>
    public Body Body { get => _Body; set => _Body = value ?? []; }

    /// <summary>Получить или установить название страницы (заполняет элемент title)</summary>
    public string Title
    {
        get => _Head.Elements.OfType<HElement>().FirstOrDefault(e => e.Name == "title")?.InnerText() ?? "";
        set
        {
            var title = _Head.Elements.OfType<Title>().FirstOrDefault();
            if (title is null)
            {
                _Head.Elements.Add(new Title(new Text(value)));
                return;
            }
            var title_elements = title.Elements;
            title_elements.Clear();
            title_elements.Add(new Text(value));
        }
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var str = $"<!DOCTYPE html>\r\n{new HElement("html", _Head, _Body)}";
        Console.WriteLine(str);
        return str;
    }
}