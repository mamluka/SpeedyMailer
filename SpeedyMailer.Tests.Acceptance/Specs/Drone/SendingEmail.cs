using System.IO;
using NUnit.Framework;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Master.Web.Core.Commands;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Tests.Acceptance.Specs.Drone
{
    [TestFixture]
    public class SendingEmail : IntegrationTestBase
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void SendCreative_ShouldStartTheSendingTaskOnTheDrone()
        {
            AddList();
            AddContacts();
        }

        private void AddList()
        {
            Master.ExecuteCommand<CreateListCommand, string>(x =>
                                                                 {
                                                                     x.Name = "Default list";
                                                                     x.Id = 1;
                                                                 });
        }

        private void AddContacts()
        {
            var stream = File.OpenRead("fixture/contacts/csv-sample.csv");
            Master.ExecuteCommand<UploadListCommand, UploadListCommandResult>(x =>
                                                                                 {
                                                                                     x.ListId = "lists/1";
                                                                                     x.Source = stream;
                                                                                 });
        }
    }

    public class CreateListCommand : Command<string>
    {
        public string Name { get; set; }
        public int Id { get; set; }

        public override string Execute()
        {
            return null;
        }
    }
}
