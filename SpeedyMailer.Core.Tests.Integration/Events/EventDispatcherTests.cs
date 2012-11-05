using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Core.Evens;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Core.IntegrationTests.Events
{
	public class EventDispatcherTests : IntegrationTestBase
	{
		private EventDispatcher _target;

		public override void ExtraSetup()
		{
			_target = MasterResolve<EventDispatcher>();
		}

		[Test]
		public void ExecuteAll_WhenExecutesAnEvent_ShouldCallAllEventRegistered()
		{
			var testEventData = new TestEventData { ResultId = "result/1", SecondResultId = "result/2" };

			_target.ExecuteAll(testEventData);

			Store.WaitForEntityToExist("result/1");
			Store.WaitForEntityToExist("result/2");

			var result1 = Store.Load<ComputationResult<int>>("result/1");
			var result2 = Store.Load<ComputationResult<int>>("result/2");

			result1.Result.Should().Be(2);
			result2.Result.Should().Be(2);
		}
	}
}
