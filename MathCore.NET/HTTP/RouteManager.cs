using System;
using System.Collections.Generic;
using System.Net;

namespace MathCore.NET.HTTP;

/// <summary>Управляет маршрутами HTTP-запросов и их обработчиками</summary>
public class RouteManager(WebServer server)
{
    //private static readonly Regex sf_RouteFormatter = new Regex(@"\?.*()");

    //private static readonly Regex __Regex = new Regex(
    //    @"(?<=(?:^/)|(?:^(?<protocol>\w+)://))(?<address>(?:(?<elements>[\w.]+)/?)+)(?=(?:\?(?:(?<params>(?:[^=]+=[^&]+))&?)*))",
    //    RegexOptions.Compiled);

    /* ---------------------------------------------------------------------------------------------------------------------------- */

    private readonly List<Route> _Routes = [];

    /* ---------------------------------------------------------------------------------------------------------------------------- */

    /// <summary>
    /// Получает или устанавливает обработчик для указанного маршрута
    /// </summary>
    /// <param name="route">Маршрут HTTP-запроса</param>
    /// <returns>Обработчик запроса для маршрута</returns>
    public Action<RequestInfo> this[string route]
    {
        get => _Routes.Find(r => r.Regex == route).Action;
        set
        {
            var rt = _Routes.Find(r => r.Regex == route);
            if (rt != null) rt.Action = value;
            else _Routes.Add(new(route, value));
        }
    }

    /* ---------------------------------------------------------------------------------------------------------------------------- */

    /// <summary>Добавляет маршрут с его обработчиком</summary>
    /// <param name="route">Маршрут HTTP-запроса</param>
    /// <param name="processor">Функция-обработчик запроса</param>
    public void Add(string route, Action<RequestInfo> processor) => _Routes.Add(new(route, processor));

    /// <summary>Добавляет несколько маршрутов одновременно</summary>
    /// <param name="routes">Массив маршрутов для добавления</param>
    public void Add(params Route[] routes) => _Routes.AddRange(routes);

    /// <summary>Обрабатывает входящий HTTP-запрос, ищет подходящий маршрут и выполняет его обработчик</summary>
    /// <param name="Context">Контекст HTTP-запроса от сервера</param>
    internal void Process(HttpListenerContext Context)
    {
        foreach (var route in _Routes)
            if (route.Execute(Context, server))
                return;
    }
}