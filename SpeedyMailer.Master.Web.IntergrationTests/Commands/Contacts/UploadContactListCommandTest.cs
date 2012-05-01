using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using CsvHelper;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Master.Web.Core.Commands;
using SpeedyMailer.Tests.Core;
using Rhino.Mocks;
using FluentAssertions;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Master.Web.IntergrationTests.Commands.Contacts
{
    [TestFixture]
    public class UploadContactListCommandTest : IntegrationTestBase
    {
        [Test]
        public void Execute_WhenAPerfectCSVListIsGiven_ShouldParseItAndWriteToDataBase()
        {
            var listId = Master.ExecuteCommand<CreateListCommand, string>(x => x.Name = "AList");

            var filename = GenerateFileName("sample", "csv");
            CreateContactsCSV(filename);

            var csvSource = File.OpenRead(filename);
            var result = Master.ExecuteCommand<UploadListCommand, UploadListCommandResult>(x =>
                                                                                  {
                                                                                      x.ListId = listId;
                                                                                      x.Source = csvSource;
                                                                                      x.Filename = filename;
                                                                                  }
                                                                              );
            result.NumberOfContacts.Should().Be(10);
            result.Filename.Should().Be(filename);
        }

        private string GenerateFileName(string seed, string extention)
        {
            return string.Format("{0}-{1}.{2}",seed,Guid.NewGuid(),extention);
        }


        [Test]
        public void Execute_WhenListContainsDuplicates_ShouldIgnoreThem()
        {
            var listId = Master.ExecuteCommand<CreateListCommand, string>(x => x.Name = "AList");

            var filename = GenerateFileName("sample", "csv");
            CreateContactsCSVWithDuplicate(filename);

            var csvSource = File.OpenRead(filename);
            var result = Master.ExecuteCommand<UploadListCommand, UploadListCommandResult>(x =>
            {
                x.ListId = listId;
                x.Source = csvSource;
                x.Filename = filename;
            }
                                                                              );
            result.NumberOfContacts.Should().Be(10);
            result.Filename.Should().Be(filename);
        }

        private void CreateContactsCSVWithDuplicate(string filename)
        {
            var list = Fixture.CreateMany<ContactFromCSVRow>(10).ToList();
            list.Add(list.Last());

            CreateCSVFile(filename, list);
        }

        public void CreateCSVFile<T>(string filename, IEnumerable<T> list) where T : class
        {
            using (var textWriter = new StreamWriter(filename))
            {
                var csvWriter = new CsvWriter(textWriter);
                csvWriter.WriteRecords(list);
            }
        }

        private void CreateContactsCSV(string filename)
        {
            var list = Fixture.CreateMany<ContactFromCSVRow>(10);
            CreateCSVFile(filename, list);
        }

    }
}
