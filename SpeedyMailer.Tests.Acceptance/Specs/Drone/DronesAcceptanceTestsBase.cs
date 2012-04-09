using NUnit.Framework;
using SpeedyMailer.Tests.Acceptance.Framework;
using nDumbster.smtp;

namespace SpeedyMailer.Tests.Acceptance.Specs.Drone
{
    public class DronesAcceptanceTestsBase : AcceptanceTestsBase
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