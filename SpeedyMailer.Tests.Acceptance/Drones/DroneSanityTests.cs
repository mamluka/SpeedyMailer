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
