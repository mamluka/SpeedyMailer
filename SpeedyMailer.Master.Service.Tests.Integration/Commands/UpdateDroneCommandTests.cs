using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Raven.Client;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Domain.Drones;
using SpeedyMailer.Tests.Core;
using Rhino.Mocks;
using FluentAssertions;
using SpeedyMailer.Tests.Core.Integration.Base;
using SpeedyMailer.Tests.Core.Unit.Base;

namespace SpeedyMailer.Master.Service.Tests.Integration.Commands
{
	[TestFixture]
	public class UpdateDroneCommandTests : IntegrationTestBase
	{
		[Test]
		public void Execute_WhenGivenADrone_ShouldUpdateItsStatusInTheStore()
		{
			ServiceActions.ExecuteCommand<UpdateDroneCommand>(x => x.DroneStatus = DroneStatus.Online);
		}
	}

	public class UpdateDroneCommand : Command
	{
		private IDocumentStore _documentStore;

		public string Identifier { get; set; }
		public DroneStatus DroneStatus { get; set; }

		public UpdateDroneCommand(IDocumentStore documentStore)
		{
			_documentStore = documentStore;
		}

		public override void Execute()
		{

		}
	}
}
