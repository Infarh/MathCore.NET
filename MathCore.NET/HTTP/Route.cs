using System;
using System.Net;
using System.Text.RegularExpressions;
// ReSharper disable ConvertToAutoPropertyWhenPossible

namespace MathCore.NET.HTTP;

public class Route(Regex regex, Action<RequestInfo> action)
{
    private static readonly Regex __RouteRegexTranslator = new(@"\{(?<name>\w+)\}", RegexOptions.Compiled);

    private static Regex CreateRegex(string route, bool IgnoreCase) =>
        new(
            __RouteRegexTranslator.Replace(route, m => $@"(?<{m.Groups["name"]}>\w+)"),
            IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);

    private Regex _Regex = regex;
    private Action<RequestInfo> _Action = action;

    public string Regex { get => _Regex.ToString(); set => _Regex = CreateRegex(value, true); }
    public Action<RequestInfo> Action { get => _Action; set => _Action = value; }

    public Route(string route, Action<RequestInfo> action, bool IgnoreCase = true)
        : this(CreateRegex(route, IgnoreCase), action) { }

    public bool Execute(HttpListenerContext context, WebServer Server)
    {
        var match = _Regex.Match(context.Request.Url.LocalPath);
        if (!match.Success) return false;
        using var request_info = new RequestInfo(context, match, Server);
        _Action(request_info);
        return true;
    }
}