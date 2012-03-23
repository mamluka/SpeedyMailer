using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Rhino.Mocks;
using FluentAssertions;
using SpeedyMailer.Tests.Acceptance.Framework;
using SpeedyMailer.EmailPool.MailDrone;
using MailDrone = SpeedyMailer.EmailPool.MailDrone.MailDrone;

namespace SpeedyMailer.Tests.Acceptance.Drones
{
    [TestFixture]
    public class DroneSanityTests : DronesAcceptanceTestsBase
    {
        [Test]
        public void Sanity_ShouldSendTheFirstBanchOfEmailsWhenActivated()
        {
            MailDrone.Main(new string[] {});
        }
    }

    public class DronesAcceptanceTestsBase:AcceptanceTestsBase
    {

    }
}
