using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Master.Service.Tests.Integration.Commands
{
    [TestFixture]
    public class InitializeServiceSettingsCommandTests:IntegrationTestBase
    {
        [Test]
        public void Execute_WhenCalled_ShouldSetTheSettings()
        {
            ServiceActions.ExecuteCommand<InitializeServiceSettingsCommand>();
        }
    }

    public class InitializeServiceSettingsCommand:Command
    {
        public string BaseUrl { get; set; }
        public override void Execute()
        {
            
        }
    }
}
