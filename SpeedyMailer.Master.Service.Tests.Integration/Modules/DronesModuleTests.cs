using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            api.Call<ServiceEndpoints.RegisterDrone>(x=> x.Identifier = "droneBaseUrl");

            WaitForEntitiesToExist<Drone>(1);
            var result = Query<Drone>().First();

            result.Hostname.Should().Be("droneBaseUrl");
        }
    }
}
