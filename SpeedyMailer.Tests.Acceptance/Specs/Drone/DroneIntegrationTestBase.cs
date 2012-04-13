using NUnit.Framework;
using SpeedyMailer.Tests.Core.Integration.Base;
using nDumbster.smtp;

namespace SpeedyMailer.Tests.Acceptance.Specs.Drone
{
    public class DroneIntegrationTestBase : IntegrationTestBase
    {
        public SimpleSmtpServer SmtpServer { get; private set; }

        [TestFixtureSetUp]
        public void DroneFixtureSetup()
        {
            SmtpServer = SimpleSmtpServer.Start();
        }

        [TestFixtureTearDown]
        public void DroneFixtureTearDown()
        {
            SmtpServer.Stop();
        }
    }
}