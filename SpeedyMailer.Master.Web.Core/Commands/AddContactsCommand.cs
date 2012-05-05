using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Client;
using Raven.Client.Exceptions;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Domain.Contacts;

namespace SpeedyMailer.Master.Web.Core.Commands
{
    public class AddContactsCommand:Command<long>
    {
        private readonly IDocumentStore _documentStore;
        public IEnumerable<Contact> Contacts { get; set; }
        public string ListId { get; set; }

        public AddContactsCommand(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }

        public override long Execute()
        {
            var counter = 0;
            using (var session = _documentStore.OpenSession())
            {
                Contacts = Contacts.Select(x =>
                                               {
                                                   x.MemberOf = new List<string> {ListId};
                                                   return x;
                                               });

                Contacts.ToList().ForEach(x =>
                                              {
                                                  try
                                                  {
                                                      session.Store(x);
                                                      session.Store(new UniqueContactEnforcer(x.Email, x.Id));
                                                      counter++;
                                                  }
                                                  catch (NonUniqueObjectException)
                                                  {
                                                      var uniqueEnforcer = session.Load<UniqueContactEnforcer>(x.Email);
                                                      var entity = session.Load<Contact>(uniqueEnforcer.EnforcedId);
                                                      if (entity.MemberOf.Contains(ListId) == false)
                                                      {
                                                          entity.MemberOf.Add(ListId);
                                                          session.Store(entity);
                                                          counter++;
                                                      }
                                                  }
                                              });
                session.SaveChanges();
                return counter;
            }
        }
    }
}