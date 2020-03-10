using System;
using System.Linq;
using MathCore.NET.HTTP;

namespace MathCore.NET.TestConsole
{
    internal static class CheckSystem
    {
        public static void Run()
        {
            foreach (var rule in WebServer.GetRules().Where(r => r.Access.Any(a => a.User.Contains(Environment.UserName))))
            {
                Console.WriteLine("> Rule: {0}", rule);
            }
        }
    }
}
