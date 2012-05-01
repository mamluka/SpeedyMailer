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
    public class UploadListCommand : Command<UploadListCommandResult>
     
    {
        private readonly IDocumentStore _store;

        public Stream Source { get; set; }
        public string ListId { get; set; }
        public string Filename { get; set; }

        public UploadListCommand(IDocumentStore store)
        {
            _store = store;
        }

        public override UploadListCommandResult Execute()
        {
            var csvReader = new CsvReader(new StreamReader(Source));
            var rows = csvReader.GetRecords<ContactFromCSVRow>().ToList();

            var numberOfProccessedLines = 0;
            
            using (var session = _store.OpenSession())
            {
                foreach (var contactFromCSVRow in rows)
                {
                    
                    try
                    {
                        var entity = new Contact
                                     {
                                         Email = contactFromCSVRow.Email, Name = contactFromCSVRow.Name, Country = contactFromCSVRow.Country, MemberOf = new List<string> {ListId}
                                     };
                        session.Store(entity);

                        session.Store(new UniqueContactEnforcer(contactFromCSVRow.Email,entity.Id));
                        Trace.WriteLine(contactFromCSVRow.Email);
                        numberOfProccessedLines++;
                    }
                    catch (NonUniqueObjectException)
                    {
                        var uniqueEnforcer = session.Load<UniqueContactEnforcer>(contactFromCSVRow.Email);
                        var entity = session.Load<Contact>(uniqueEnforcer.EnforcedId);
                        if (entity.MemberOf.Contains(ListId)==false)
                        {
                            entity.MemberOf.Add(ListId);
                            session.Store(entity);
                            numberOfProccessedLines++;
                        }
                    }
                    
                }
                session.SaveChanges();
            }

            return new UploadListCommandResult
                       {
                           NumberOfContacts = numberOfProccessedLines,
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
