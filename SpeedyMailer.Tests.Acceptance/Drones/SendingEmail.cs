using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SpeedyMailer.Master.Service;
using SpeedyMailer.Tests.Core;
using Rhino.Mocks;
using FluentAssertions;

namespace SpeedyMailer.Tests.Acceptance.Drones
{
    [TestFixture]
    public class SendingEmail : AutoMapperAndFixtureBase
    {
        [Test]
        public void SendingAnEmailThroughTheApi_ShouldMakeTheDroneSendTheEmailsSpecifiedByTheService()
        {
            
        }
    }
}
