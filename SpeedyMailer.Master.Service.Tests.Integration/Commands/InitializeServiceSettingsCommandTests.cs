using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Master.Service.Commands;
using SpeedyMailer.Tests.Core.Integration.Base;
using FluentAssertions;

namespace SpeedyMailer.Master.Service.Tests.Integration.Commands
{
    [TestFixture]
    public class InitializeServiceSettingsCommandTests:IntegrationTestBase
    {
        [Test]
        public void Execute_WhenCalled_ShouldSetTheSettings()
        {
            ServiceActions.ExecuteCommand<InitializeServiceSettingsCommand>(x=>
	            {
		            x.BaseUrl = "baseurl";
		            x.DatabaseUrl = "databaseUrl";
	            });

	        var serviceSettings = MasterResolve<ServiceSettings>();

	        serviceSettings.BaseUrl.Should().Be("baseurl");
        }
    }
}
