using System;
using System.Collections.Generic;
using System.Net;

namespace MathCore.NET.HTTP
{
    public class RouteManager
    {
        //private static readonly Regex sf_RouteFormatter = new Regex(@"\?.*()");

        //private static readonly Regex sf_Regex = new Regex(
        //    @"(?<=(?:^/)|(?:^(?<protocol>\w+)://))(?<address>(?:(?<elements>[\w.]+)/?)+)(?=(?:\?(?:(?<params>(?:[^=]+=[^&]+))&?)*))",
        //    RegexOptions.Compiled);

        /* ---------------------------------------------------------------------------------------------------------------------------- */

        private readonly List<Route> _Routes = new();
        private readonly WebServer _Server;

        /* ---------------------------------------------------------------------------------------------------------------------------- */

        public Action<RequestInfo> this[string route]
        {
            get => _Routes.Find(r => r.Regex == route)?.Action;
            set
            {
                var rt = _Routes.Find(r => r.Regex == route);
                if (rt != null) rt.Action = value;
                else _Routes.Add(new Route(route, value));
            }
        }

        /* ---------------------------------------------------------------------------------------------------------------------------- */

        public RouteManager(WebServer server) => _Server = server;

        /* ---------------------------------------------------------------------------------------------------------------------------- */

        public void Add(string route, Action<RequestInfo> processor) => _Routes.Add(new Route(route, processor));

        public void Add(params Route[] routes) => _Routes.AddRange(routes);

        internal void Process(HttpListenerContext Context)
        {
            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var route in _Routes)
                if (route.Execute(Context, _Server))
                    return;
        }
    }
}