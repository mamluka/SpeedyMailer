using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Core.Rules;
using SpeedyMailer.Master.Service.Commands;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Master.Service.Tests.Integration.Commands
{
	public class AddIntervalRuleCommandTests : IntegrationTestBase
	{
		[Test]
		public void Execute_WhenRuleIsGiven_ShouldStoreIt()
		{
			var rule = new IntervalRule
						   {
							   Conditon = new List<string> { "gmai.com" },
							   Interval = 20
						   };
			ServiceActions.ExecuteCommand<AddIntervalRulesCommand>(x => x.Rules = new[] { rule });

			WaitForEntitiesToExist<IntervalRule>(1);

			var result = Query<IntervalRule>().First();

			result.Conditon.Should().Contain(new[] {"gmail.com"});
			rule.Interval.Should().Be(20);
		}
	}
}
