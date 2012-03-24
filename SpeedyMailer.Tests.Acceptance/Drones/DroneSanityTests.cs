using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Rhino.Mocks;
using FluentAssertions;
using SpeedyMailer.Tests.Acceptance.Framework;
using SpeedyMailer.EmailPool.MailDrone;
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

        [Test]
        public void Method_ShouldStory()
        {
            var email = new MailMessage("david@david.com", "test@test.com");
            email.Subject = "hello";

            var smptClient = new SmtpClient("localhost", 25) {Timeout = 3000};
            smptClient.Send(email);


            SimpleSmtpServer.ReceivedEmailCount.Should().Be(1);

        }
    }

    public class DronesAcceptanceTestsBase:AcceptanceTestsBase
    {
        private SimpleSmtpServer _simpleSmtpServer;

        public SimpleSmtpServer SimpleSmtpServer
        {
            get { return _simpleSmtpServer; }
        }

        [TestFixtureSetUp]
        public void DroneFixtureSetup()
        {
            _simpleSmtpServer = SimpleSmtpServer.Start();
        }

        [TestFixtureTearDown]
        public void DroneFixtureTearDown()
        {
            SimpleSmtpServer.Stop();
        }
    }
}
