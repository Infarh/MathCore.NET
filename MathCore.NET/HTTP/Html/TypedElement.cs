using System;

namespace MathCore.NET.HTTP.Html
{
    public abstract class TypedElement : HElement
    {
        /// <inheritdoc />
        public override string Name { get => base.Name; set => throw new NotSupportedException("Изменить имя типизированного элемента нельзя"); }

        protected TypedElement(string Name, params HElementBase[] elements) : base(Name, elements) { }
    }
}