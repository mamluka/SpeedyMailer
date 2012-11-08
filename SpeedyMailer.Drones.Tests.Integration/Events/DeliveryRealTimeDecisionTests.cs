using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Drones.Commands;
using SpeedyMailer.Drones.Events;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Drones.Tests.Integration.Events
{
	public class DeliveryRealTimeDecisionTests : IntegrationTestBase
	{
		[Test]
		public void Inspect_WhenGivenABounceEventAndGroupCountIsNotEnoughToStopTheSendingGroup_ShouldNotStopSending()
		{
			DroneActions.ExecuteCommand<UpdateDomainGroupBounceCountersCommand,IList<Bounce>>();
			
			//FireEvent<DeliveryRealTimeDecision, MailEvent>();
		}
	}
}
