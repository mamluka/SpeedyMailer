using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;
using SpeedyMailer.Master.Service;
using SpeedyMailer.Master.Web.Core;
using SpeedyMailer.Master.Web.Core.Commands;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Master.Web.IntergrationTests.Commands
{
    [TestFixture]
    public class SendCreativeCommandTests : IntegrationTestBase
    {
		[Test]
		public void Execute_WhenGivenAnEmailId_SendApiRequestToMasterService()
		{
			Service.Start();
			Service.EditSettings<ICreativeApisSettings>(x=> x.AddCreative = "/api/1");

			const string creativeId = "email/1";

			UI.ExecuteCommand<SendCreativeCommand>(x =>
			                                           	{
			                                           		x.CreativeId = creativeId;
			                                           	});

			Service.Stop();
		}
    }
}
