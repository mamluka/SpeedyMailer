using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Tests.Core;
using Rhino.Mocks;
using FluentAssertions;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Master.Service.Tests.Integration.Commands
{
	[TestFixture]
	public class WakeDronesUpCommandTests :IntegrationTestBase
	{
		[Test]
		public void Execute_WhenCalled_ShouldSendAAwakeUpApiCallToAllDronesRegistered()
		{
			var result = Service.ExecuteCommand<WakeupDronesCommand, IList<Drone>>(x=>
			                                                                       	{
			                                                                       		
			                                                                       	});

		}
	}

	public class WakeupDronesCommand:Command<IList<Drone>>
	{
		public override IList<Drone> Execute()
		{
			return null;
		}
	}

	public class Drone
	{
		
	}
}
