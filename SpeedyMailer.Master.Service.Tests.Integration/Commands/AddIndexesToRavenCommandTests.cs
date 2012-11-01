using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Master.Service.Commands;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Master.Service.Tests.Integration.Commands
{
	public class AddIndexesToRavenCommandTests : IntegrationTestBase
	{
		[Test]
		public void Execute_WhenExecuted_ShouldPutAllTheIndexesInTheDataBase()
		{
			ServiceActions.ExecuteCommand<AddIndexesToRavenCommand>();

			var indexes = DocumentStore.DatabaseCommands.GetIndexNames(0, 100);

			indexes.Should().BeEquivalentTo(new[] { "Contacts/DomainGroupCounter", "Contacts/ByMemberOf" });
		}
	}
}
