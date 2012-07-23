using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Drones.Commands;
using SpeedyMailer.Drones.Settings;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Drones.Tests.Integration.Commands
{
	public class SendCreativePackageCommandTests : IntegrationTestBase
	{
		[Test]
		public void Execute_WhenToldToWriteToDisk_ShouldWriteTheActualEmailToDisk()
		{
			DroneActions.EditSettings<IEmailingSettings>(x => x.WriteEmailsToDisk = true);

			var creativePackage = new CreativePackage
			                      	{
			                      		
			                      	};
			DroneActions.ExecuteCommand<SendCreativePackageCommand>(x=> x.Package = creativePackage);
		}
	}
}
