using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathCore.NET.HTTP.Html;

/// <summary>Базовый класс HTML-элемента с поддержкой вложенных элементов и атрибутов</summary>
public class HElement : HElementBase, IEnumerable<HElementBase>, IEnumerable<HAttribute>
{
    /// <summary>Имя HTML-элемента</summary>
    public virtual string Name { get; set; }

    /// <summary>Коллекция вложенных элементов</summary>
    public List<HElementBase> Elements { get; set; } = new();

    /// <summary>Проверка наличия вложенных элементов</summary>
    public bool HasElements => Elements.Count > 0;

    /// <summary>Коллекция атрибутов элемента</summary>
    public List<HAttribute> Attributes { get; set; } = new();

    /// <summary>Проверка наличия атрибутов</summary>
    public bool HasAttributes => Attributes.Count > 0;

    /// <summary>Всегда ли открывать тег (не закрывается /&gt;)</summary>
    public virtual bool AlwaysOpen { get; set; }

    /// <summary>Только открывающий тег (не закрывается)</summary>
    public virtual bool OnlyOpen { get; set; }

    /// <summary>Инициализирует новый экземпляр HTML-элемента</summary>
    /// <param name="Name">Имя элемента</param>
    /// <param name="elements">Вложенные элементы</param>
    public HElement(string Name, params HElementBase[] elements)
    {
        this.Name = Name;
        if (elements.Length > 0) Elements = elements.ToList();
    }

    /// <summary>Добавить атрибуты к элементу</summary>
    /// <param name="attribute">Атрибуты для добавления</param>
    public void Add(params HAttribute[] attribute) => Attributes.AddRange(attribute);

    /// <summary>Добавить вложенные элементы</summary>
    /// <param name="element">Элементы для добавления</param>
    public void Add(params HElementBase[] element) => Elements.AddRange(element);

    /// <summary>Добавить объекты (автоматически преобразуются в соответствующие элементы/атрибуты)</summary>
    /// <param name="items">Объекты для добавления</param>
    public void Add(params object[] items)
    {
        foreach (var item in items)
        {
            switch (item)
            {
                case HAttribute attribute:
                    Attributes.Add(attribute);
                    break;
                case HElement element:
                    Elements.Add(element);
                    break;
                case string str:
                    Elements.Add(new Text(str));
                    break;
                default:
                    Elements.Add(new Text(item.ToString()));
                    break;
            }
        }
    }

    /// <summary>Получить внутренний HTML-код</summary>
    /// <returns>HTML-код вложенных элементов</returns>
    public string InnerHtml() => InnerHtml(0);

    /// <summary>Получить внутренний HTML-код с указанным уровнем отступа</summary>
    /// <param name="level">Уровень отступа</param>
    /// <returns>HTML-код вложенных элементов</returns>
    protected string InnerHtml(int level) => HasElements
        ? string.Join("\r\n", Elements.Select(e => e.ToString(level + 1)))
        : string.Empty;

    /// <inheritdoc />
    public override string InnerText() => InnerText(0);

    /// <summary>Получить внутренний текст с указанным уровнем отступа</summary>
    /// <param name="level">Уровень отступа</param>
    /// <returns>Внутренний текст</returns>
    protected string InnerText(int level) => HasElements
        ? string.Join($"{GetSpacer(level + 1)}\r\n", Elements.Select(e => e.InnerText()))
        : string.Empty;

    /// <inheritdoc />
    public override string ToString(int level)
    {
        var spacer = GetSpacer(level);
        var result = new StringBuilder($"{spacer}<{Name}");
        if (HasAttributes) result.AppendFormat(" {0}", string.Join(" ", Attributes));

        if (!HasElements)
            return OnlyOpen
                ? $"{result}>"
                : AlwaysOpen
                    ? $"{result}></{Name}>"
                    : $"{result}/>";

        if (Elements.Count == 1)
        {
            var inner_text = InnerHtml(level);
            if (!inner_text.Contains("\r\n"))
                return $"{result}>{inner_text.Trim()}</{Name}>";
        }

        result.AppendLine(">");
        result.AppendLine(InnerHtml(level));
        result.AppendFormat("{1}</{0}>", Name, spacer);

        return result.ToString();
    }

    /// <inheritdoc />
    public IEnumerator<HElementBase> GetEnumerator() => Elements.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Elements).GetEnumerator();

    /// <inheritdoc />
    IEnumerator<HAttribute> IEnumerable<HAttribute>.GetEnumerator() => Attributes.GetEnumerator();
}