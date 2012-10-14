using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using FluentAssertions;

namespace SpeedyMailer.Tests.Core.Integration.Base
{
	public static class TestsExtentions
	{
		public static void AssertTimeDifferenceInRange(this IEnumerable<DateTime> target, decimal interval, decimal tolorance)
		{
			if (tolorance == interval)
				tolorance = tolorance * (2 / 3);

			var deltas = GetDelta(target);
			deltas.Should().OnlyContain(x => (decimal)x.TotalSeconds > interval - tolorance && (decimal)x.TotalSeconds < interval + tolorance);
		}

		private static IEnumerable<TimeSpan> GetDelta(IEnumerable<DateTime> target)
		{
			var orderTarget = target.OrderBy(x => x.ToUniversalTime());

			var deltas = orderTarget.Zip(orderTarget.Skip(1), (current, next) => next - current);
			return deltas;
		}

		public static void AssertTimesAreTheSameInRange(this IEnumerable<DateTime> target, decimal range)
		{
			var deltas = GetDelta(target);
			deltas.Should().OnlyContain(x => (decimal)x.TotalMilliseconds < range);
		}
	}
}
