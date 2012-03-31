using NUnit.Framework;
using SpeedyMailer.Master.Web.UI;

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
}