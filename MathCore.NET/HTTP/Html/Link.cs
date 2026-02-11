using System;
using System.Collections.Generic;
using System.Linq;

namespace MathCore.NET.HTTP.Html;

/// <summary>HTML-элемент link (ссылка на ресурс)</summary>
public class Link : TypedElement
{
    /// <summary>Только открывающий тег</summary>
    public override bool OnlyOpen { get => true; set => throw new NotSupportedException(); }

    /// <summary>Отношение к ресурсу (rel)</summary>
    public string Relation
    {
        get => Attributes.FirstOrDefault(a => a.AttributeName.Equals("rel", StringComparison.InvariantCultureIgnoreCase))?.Value ?? "";
        set
        {
            var attribute = Attributes.FirstOrDefault(a => a.AttributeName.Equals("rel", StringComparison.InvariantCultureIgnoreCase));
            if (attribute != null) attribute.Value = value;
            else Attributes.Add(new("rel", value));
        }
    }

    /// <summary>Ссылка на ресурс (href)</summary>
    public string Reference
    {
        get => Attributes.FirstOrDefault(a => a.AttributeName.Equals("href", StringComparison.InvariantCultureIgnoreCase))?.Value ?? "";
        set
        {
            var attribute = Attributes.FirstOrDefault(a => a.AttributeName.Equals("href", StringComparison.InvariantCultureIgnoreCase));
            if (attribute != null) attribute.Value = value;
            else Attributes.Add(new("href", value));
        }
    }

    /// <summary>Инициализирует новый экземпляр Link</summary>
    public Link() : base("link") { }

    /// <summary>Инициализирует новый экземпляр Link с указанным отношением и ссылкой</summary>
    /// <param name="relation">Отношение к ресурсу</param>
    /// <param name="href">Ссылка на ресурс</param>
    public Link(string relation, string href) : base("link")
    {
        List<HAttribute>? attributes = null;
        if (!string.IsNullOrWhiteSpace(relation)) (attributes = Attributes).Add(new("rel", relation));
        if (!string.IsNullOrWhiteSpace(href)) (attributes ?? Attributes).Add(new("href", href));
    }
}