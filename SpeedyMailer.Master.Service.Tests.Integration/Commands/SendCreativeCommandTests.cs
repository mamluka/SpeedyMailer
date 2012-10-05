using NUnit.Framework;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Master.Service.Commands;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Master.Service.Tests.Integration.Commands
{
    [TestFixture]
    public class SendCreativeCommandTests : IntegrationTestBase
    {
        [Test]
        public void Execute_WhenGivenACreativeId_CallsTheEndpointWithTheCreativeId()
        {
            ServiceActions.EditSettings<ApiCallsSettings>(x => x.ApiBaseUri = DefaultBaseUrl);

            const string creativeId = "creative/1";
            ListenToApiCall<ServiceEndpoints.Send>();

            UIActions.ExecuteCommand<SendCreativeCommand,ApiResult>(x =>
                                                                {
                                                                    x.CreativeId = creativeId;
                                                                });

            AssertApiCalled<ServiceEndpoints.Send>(x=> x.CreativeId == creativeId);
        }
    }
}
