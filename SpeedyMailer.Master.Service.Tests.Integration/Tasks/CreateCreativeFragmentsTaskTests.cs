using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using FluentAssertions;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Master.Service.Tasks;
using SpeedyMailer.Master.Web.Core.Commands;
using SpeedyMailer.Tests.Core;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Master.Service.Tests.Integration.Tasks
{
    [TestFixture]
    public class CreateCreativeFragmentsTaskTests : IntegrationTestBase
    {
        [Test]
        public void Execute_WhenACreativeIsGiven_ShouldStoreTheFragmentsWithRecipientsDevidedBetweenTheFragments()
        {

            var listId = UIActions.CreateListWithRandomContacts("MyList", 1500);
            
            const string templateBody = "Body";
            var templateId = CreateTemplate(templateBody);

            var creativeId = UIActions.ExecuteCommand<AddCreativeCommand,string>(x=>
                                                      {
                                                          x.Body = "Body";
                                                          x.Subject = "Subject";
                                                          x.UnsubscribeTemplateId = templateId;
                                                          x.Lists = new List<string> {listId};
                                                      });

            var task = new CreateCreativeFragmentsTask
                        {
                            CreativeId = creativeId,
                            RecipientsPerFragment = 1000
                        };

            ServiceActions.ExecuteTask(task);

            var result = Query<CreativeFragment>(x => x.CreativeId == creativeId);

            result.Should().HaveCount(2);
            result.First().Recipients.Should().HaveCount(1000);
            result.Second().Recipients.Should().HaveCount(500);

            result.First().UnsubscribeTemplate.Should().Be(templateBody);
            result.Second().UnsubscribeTemplate.Should().Be(templateBody);
        }

        private string CreateTemplate(string templateBody)
        {
            var templateId = UIActions.ExecuteCommand<CreateTemplateCommand, string>(x => { x.Body = templateBody; });
            return templateId;
        }
    }
}
