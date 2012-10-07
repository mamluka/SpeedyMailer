using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;

namespace SpeedyMailer.Tests.Core.Integration.Base
{
	public static class TestsExtentions
	{
		public static void AssertTimeDifferenceInRange(this IEnumerable<DateTime> target, int interval, int tolorance)
		{
			target.Zip(target.Skip(1), (current, next) => next - current).Should().OnlyContain(x => x.TotalSeconds > interval - tolorance && x.TotalSeconds < interval + tolorance);
		}
	}
}
