using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeedyMailer.Tests.Core
{
    public static class TestingExtentions
    {
        public static T Second<T>(this IEnumerable<T> enumerable )
        {
            return enumerable.ToList()[1];
        }

        public static T Third<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.ToList()[2];
        }
    }
}
