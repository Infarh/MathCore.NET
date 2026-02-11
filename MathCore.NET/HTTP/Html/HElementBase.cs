using System.Linq;

namespace MathCore.NET.HTTP.Html;

/// <summary>Базовый класс для HTML-элементов</summary>
public abstract class HElementBase
{
    /// <summary>Строка отступа, используемая при форматировании</summary>
    public static string IdentPattern { get; set; } = "    ";

    /// <summary>Получить отступ для указанного уровня</summary>
    /// <param name="level">Уровень отступа</param>
    /// <returns>Строка отступа</returns>
    protected static string GetSpacer(int level) => level <= 0
        ? string.Empty
        : string.Concat(Enumerable.Repeat(IdentPattern, level));

    /// <summary>Получить внутренний текст элемента</summary>
    /// <returns>Внутренний текст</returns>
    public abstract string InnerText();

    /// <summary>Получить строковое представление элемента с указанным уровнем отступа</summary>
    /// <param name="level">Уровень отступа</param>
    /// <returns>Строковое представление</returns>
    public abstract string ToString(int level);

    /// <inheritdoc />
    public override string ToString() => ToString(0);

    /// <summary>Неявное преобразование строки в HTML-элемент Text</summary>
    /// <param name="text">Текст для преобразования</param>
    /// <returns>Элемент Text</returns>
    public static implicit operator HElementBase(string text) => new Text(text);
}