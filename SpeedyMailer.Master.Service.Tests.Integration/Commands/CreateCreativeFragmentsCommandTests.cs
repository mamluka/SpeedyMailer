using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using FluentAssertions;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Master.Service.Commands;
using SpeedyMailer.Master.Web.Core.Commands;
using SpeedyMailer.Tests.Core;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Master.Service.Tests.Integration.Commands
{
    [TestFixture]
    public class CreateCreativeFragmentsCommandTests : IntegrationTestBase
    {
        [Test]
        public void Execute_WhenACreativeIsGiven_ShouldStoreTheFragmentsWithRecipientsDevidedBetweenTheFragments()
        {

            var listId = UI.CreateAListWithRandomContacts("MyList", 1500);
            
            const string templateBody = "Body";
            var templateId = CreateTemplate(templateBody);

            var creativeId = UI.ExecuteCommand<AddCreativeCommand,string>(x=>
                                                      {
                                                          x.Body = "Body";
                                                          x.Subject = "Subject";
                                                          x.UnsubscribeTemplateId = templateId;
                                                          x.Lists = new List<string> {listId};
                                                      });

            Service.ExecuteCommand<CreateCreativeFragmentsCommand>(x =>
                                                                       {
                                                                           x.CreativeId = creativeId;
                                                                           x.RecipientsPerFragment = 1000;
                                                                       });

            var result = Query<CreativeFragment>(x => x.Creative.Id == creativeId);

            result.Should().HaveCount(2);
            result.First().Recipients.Should().HaveCount(1000);
            result.Second().Recipients.Should().HaveCount(500);

            result.First().UnsubscribeTemplate.Should().Be(templateBody);
            result.Second().UnsubscribeTemplate.Should().Be(templateBody);
        }

        private string CreateTemplate(string templateBody)
        {
            var templateId = UI.ExecuteCommand<CreateTemplateCommand, string>(x => { x.Body = templateBody; });
            return templateId;
        }
    }
}
