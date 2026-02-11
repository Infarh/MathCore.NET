using System;

namespace MathCore.NET.HTTP.Html;

/// <summary>Типизированный HTML-элемент с предопределённым именем</summary>
public abstract class TypedElement(string Name, params HElementBase[] elements) : HElement(Name, elements)
{
    /// <summary>Имя типизированного элемента</summary>
    /// <remarks>Нельзя изменить имя типизированного элемента</remarks>
    public override string Name { get => base.Name; set => throw new NotSupportedException("Изменить имя типизированного элемента нельзя"); }
}