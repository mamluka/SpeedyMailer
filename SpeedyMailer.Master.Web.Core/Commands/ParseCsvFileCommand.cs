using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using Raven.Client;
using Raven.Client.Exceptions;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Domain.Contacts;

namespace SpeedyMailer.Master.Web.Core.Commands
{
    public class ParseCsvFileCommand : Command<UploadListCommandResult>
     
    {
        private readonly IDocumentStore _store;
        private readonly AddContactsCommand _addContactsCommand;

        public Stream Source { get; set; }
        public string ListId { get; set; }
        public string Filename { get; set; }

        public ParseCsvFileCommand(IDocumentStore store,AddContactsCommand addContactsCommand)
        {
            _addContactsCommand = addContactsCommand;
            _store = store;
        }

        public override UploadListCommandResult Execute()
        {
            var csvReader = new CsvReader(new StreamReader(Source));
            var rows = csvReader.GetRecords<ContactFromCSVRow>().ToList();

            var contacts = rows.Select(x => new Contact
                                                {
                                                    Country = x.Country,
                                                    Email = x.Email,
                                                    Name = x.Name,
                                                });

            _addContactsCommand.Contacts = contacts;
            _addContactsCommand.ListId = ListId;
            var counter = _addContactsCommand.Execute();

            return new UploadListCommandResult
                       {
                           NumberOfContacts = counter,
                           Filename = Filename
                           
                       };
            
        }
    }

    public class UniqueContactEnforcer : UniqueEnforcer
    {
        public UniqueContactEnforcer(string uniqueId, string enforcedId) : base(uniqueId, enforcedId)
        {}
    }

    public class UniqueEnforcer
    {
        public string Id { get; set; }
        public string EnforcedId { get; set; }

        public UniqueEnforcer(string uniqueId,string enforcedId)
        {
            Id = uniqueId;
            EnforcedId = enforcedId;
        }
    }


    public class UploadListCommandResult
    {
        public string Filename { get; set; }
        public long NumberOfContacts { get; set; }
    }
}
