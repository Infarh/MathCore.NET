﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MathCore.NET.Extensions
{
    internal static class EnumerableExtensions
    {
        public static IEnumerable Concat(this IEnumerable items, IEnumerable other)
        {
            foreach (var item in items) yield return item;
            foreach (var item in other) yield return item;
        }
    }
}
