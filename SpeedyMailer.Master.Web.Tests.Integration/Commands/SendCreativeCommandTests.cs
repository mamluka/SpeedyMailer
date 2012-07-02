using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SpeedyMailer.Core.Api;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Protocol;
using SpeedyMailer.Master.Web.Core.Commands;
using SpeedyMailer.Tests.Core;
using SpeedyMailer.Tests.Core.Integration.Base;
using FluentAssertions;

namespace SpeedyMailer.Master.Web.Tests.Integration.Commands
{
    [TestFixture]
    public class SendCreativeCommandTests : IntegrationTestBase
    {
        [Test]
        public void Execute_WhenGivenACreativeId_CallsTheEndpointWithTheCreativeId()
        {
        	const string creativeId = "creative/1";
			ListenToApiCall<CreativeEndpoint.Add,CreativeEndpoint.Add.Request>();

            UIActions.ExecuteCommand<SendCreativeCommand,ApiResult>(x =>
                                                             	{
                                                             		x.CreativeId = creativeId;
                                                             	});

			AssertApiCalled<CreativeEndpoint.Add.Request>(x=> x.CreativeId == creativeId);
        }
    }
}
