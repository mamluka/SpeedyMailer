using System.Linq;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Master.Web.Core.Commands;
using FluentAssertions;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Master.Web.Tests.Integration.Commands
{
    [TestFixture]
    public class AddContactsCommandTests : IntegrationTestBase
    {
        [Test]
        public void Execute_WhenGivenAListOfContactsAndAList_ShouldAddThemToTheStore()
        {

            const string listName = "MyList";
            var listId = UIActions.ExecuteCommand<CreateListCommand,string>(x =>
                                                                         {
                                                                             x.Name = listName;
                                                                         });
            var contacts = Fixture.CreateMany<Contact>(10).ToList();

            UIActions.ExecuteCommand<AddContactsCommand,long>(x=>
                                                      {
                                                          x.Contacts = contacts;
                                                          x.ListId = listId;

                                                      });

            var result = Query<Contact>(x => x.MemberOf.Any(list=> list == listId));

            var resultNames = result.Select(x => x.Name).ToList();
            var names = contacts.Select(x => x.Name).ToList();

            resultNames.Should().BeEquivalentTo(names);
        }

        [Test]
        public void Execute_WhenWeHaveDuplicates_ShouldRemovetheDuplicates()
        {
            const string listName = "MyList";
            var listId = UIActions.ExecuteCommand<CreateListCommand, string>(x =>
            {
                x.Name = listName;
            });

            var contacts = Fixture.CreateMany<Contact>(10).ToList();
            var theDuplicate = contacts[9];

            contacts.Add(theDuplicate);

            UIActions.ExecuteCommand<AddContactsCommand, long>(x =>
            {
                x.Contacts = contacts;
                x.ListId = listId;

            });

            var result = Query<Contact>(x => x.MemberOf.Any(list => list == listId));

            result.Should().HaveCount(10);
            result.Count(x => x.Name == theDuplicate.Name).Should().Be(1);

        }
    }
}
