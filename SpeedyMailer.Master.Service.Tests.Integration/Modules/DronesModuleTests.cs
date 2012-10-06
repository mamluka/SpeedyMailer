using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Drones;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Master.Service.Tests.Integration.Modules
{
    public class DronesModuleTests:IntegrationTestBase
    {
        [Test]
        public void RegisterDrone_WhenCalledWithADroneIdentifier_ShouldRegisterTheDroneInTheStore()
        {
            ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });

            ServiceActions.Initialize();
            ServiceActions.Start();

            DroneActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });
            var api = DroneResolve<Api>();

            api.Call<ServiceEndpoints.Drones.RegisterDrone>(x=>
	            {
		            x.Identifier = "droneip";
		            x.BaseUrl = "baseurl";
		            x.LastUpdate = DateTime.UtcNow.ToLongTimeString();
	            });

            WaitForEntitiesToExist<Drone>(1);
            var result = Query<Drone>().First();

            result.BaseUrl.Should().Be("baseurl");
            result.Id.Should().Be("droneip");
	        result.LastUpdated.Should().BeAfter(DateTime.UtcNow.AddSeconds(-30));
        }
    }
}
