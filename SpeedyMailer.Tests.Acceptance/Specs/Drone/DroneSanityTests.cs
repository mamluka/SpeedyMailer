using NUnit.Framework;
using SpeedyMailer.Master.Web.UI;

namespace SpeedyMailer.Tests.Acceptance.Specs.Drone
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
}