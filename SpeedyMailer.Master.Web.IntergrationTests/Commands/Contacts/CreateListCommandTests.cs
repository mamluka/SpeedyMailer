using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Rhino.Mocks;
using FluentAssertions;
using SpeedyMailer.Core.Domain.Lists;
using SpeedyMailer.Master.Web.Core.Commands;
using SpeedyMailer.Tests.Acceptance.Specs.Drone;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Master.Web.IntergrationTests.Commands.Contacts
{
    [TestFixture]
    public class CreateListCommandTests : IntegrationTestBase
    {
        [Test]
        public void Execute_WhenCalled_ShouldCreateAList()
        {
            var id = Master.ExecuteCommand<CreateListCommand, string>(x =>
                                                                 {
                                                                     x.Name = "Default";
                                                                 });

            var result = Load<ListDescriptor>(id);

            result.Name.Should().Be("Default");
        }
    }
}
