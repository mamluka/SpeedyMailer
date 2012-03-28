using System.Net.Mail;
using NUnit.Framework;
using FluentAssertions;
using SpeedyMailer.Master.Web.UI;
using SpeedyMailer.Tests.Acceptance.Framework;
using nDumbster.smtp;

namespace SpeedyMailer.Tests.Acceptance.Drones
{
    [TestFixture]
    public class DroneSanityTests : DronesAcceptanceTestsBase
    {

        [Test]
        public void Sanity_ShouldSendTheFirstBanchOfEmailsWhenActivated()
        {
            MailDroneHost.Main(new string[] {});
        }
    }

    public class DronesAcceptanceTestsBase:AcceptanceTestsBase
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
