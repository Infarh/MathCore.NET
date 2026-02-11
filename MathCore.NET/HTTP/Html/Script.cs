using System;
using System.Linq;

namespace MathCore.NET.HTTP.Html;

/// <summary>HTML-элемент script (скрипт)</summary>
public class Script : TypedElement
{
    /// <summary>Всегда открывающий тег</summary>
    public override bool AlwaysOpen { get => true; set => throw new NotSupportedException(); }

    /// <summary>Источник скрипта (src)</summary>
    public string Source
    {
        get => Attributes.FirstOrDefault(a => a.AttributeName.Equals("src", StringComparison.InvariantCultureIgnoreCase))?.Value ?? "";
        set
        {
            var attribute = Attributes.FirstOrDefault(a => a.AttributeName.Equals("src", StringComparison.InvariantCultureIgnoreCase));
            if (attribute != null) attribute.Value = value;
            else Attributes.Add(new("src", value));
        }
    }

    /// <summary>Инициализирует новый экземпляр Script</summary>
    public Script() : base("script") { }

    /// <summary>Инициализирует новый экземпляр Script с содержимым</summary>
    /// <param name="script">Содержимое скрипта</param>
    public Script(string script) : base("script", new Text(script)) { }
}