using System;

namespace MathCore.NET.HTTP.Html;

public abstract class TypedElement(string Name, params HElementBase[] elements) : HElement(Name, elements)
{
    /// <inheritdoc />
    public override string Name { get => base.Name; set => throw new NotSupportedException("Изменить имя типизированного элемента нельзя"); }
}