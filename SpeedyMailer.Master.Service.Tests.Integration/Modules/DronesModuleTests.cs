using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Drones;
using SpeedyMailer.Core.Domain.Mail;
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
		            x.Domain = "example.com";
	            });

            Store.WaitForEntitiesToExist<Drone>();
            var result = Store.Query<Drone>().First();

            result.BaseUrl.Should().Be("baseurl");
            result.Id.Should().Be("droneip");
	        result.LastUpdated.Should().BeAfter(DateTime.UtcNow.AddSeconds(-30));
	        result.Domain.Should().Be("example.com");
        }

		[Test]
		public void GetDnsblData_WhenCalled_ShouldReturnTheListOfDnsblServices()
		{
			ServiceActions.EditSettings<ServiceSettings>(x => { x.BaseUrl = DefaultBaseUrl; });

			ServiceActions.Initialize();
			ServiceActions.Start();

			DroneActions.EditSettings<ApiCallsSettings>(x => { x.ApiBaseUri = DefaultBaseUrl; });

			var api = DroneResolve<Api>();

			var result = api.Call<ServiceEndpoints.Drones.GetDnsblData,List<Dnsbl>>();

			result.Should().HaveCount(27);
			result[0].Dns.Should().Be("virbl.Dnsbl.bit.nl");
			result[0].Name.Should().Be("VIRBL");
			result[0].Type.Should().Be(DnsnlType.Ip);
		}

    }
}
