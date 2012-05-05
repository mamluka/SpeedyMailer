using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Rhino.Mocks;
using FluentAssertions;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Master.Service.Commands;
using SpeedyMailer.Master.Web.Core.Commands;
using SpeedyMailer.Master.Web.IntergrationTests.Commands;
using SpeedyMailer.Tests.Core;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Master.Service.Intergration.Tests.Commands
{
    [TestFixture]
    public class CreateCreativeFragmentsCommandTests : IntegrationTestBase
    {
        [Test]
        public void Execute_WhenACreativeIsGiven_ShouldStoreTheFragmentsWithRecipientsDevidedBetweenTheFragments()
        {
            var listId = UI.ExecuteCommand<CreateListCommand,string>(x => x.Name = "MyList");
            var creativeId = UI.ExecuteCommand<AddCreativeCommand,string>(x=>
                                                      {
                                                          x.Body = "Body";
                                                          x.Subject = "Subject";
                                                          x.Lists = new List<string> {listId};
                                                      });

            Service.ExecuteCommand<CreateCreativeFragmentsCommand>(x => x.CreativeId = creativeId);

            var result = Query<CreativeFragment>(x => x.Creative.Id == creativeId);

            result.Should().HaveCount(2);
            result.First().Recipients.Should().HaveCount(1000);
            result.Second().Recipients.Should().HaveCount(1000);
        }
    }
}
