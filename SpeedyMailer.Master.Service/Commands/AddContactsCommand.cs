using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Client;
using Raven.Client.Exceptions;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Rules;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Master.Service.Tasks;

namespace SpeedyMailer.Master.Service.Commands
{
    public class AddContactsCommand:Command<long>
    {
        private readonly IDocumentStore _documentStore;
	    private readonly CreativeFragmentSettings _creativeFragmentSettings;
	    public IEnumerable<Contact> Contacts { get; set; }
        public string ListId { get; set; }

        public AddContactsCommand(IDocumentStore documentStore,CreativeFragmentSettings creativeFragmentSettings)
        {
	        _creativeFragmentSettings = creativeFragmentSettings;
	        _documentStore = documentStore;
        }

	    public override long Execute()
        {
            var counter = 0;
            using (var session = _documentStore.OpenSession())
            {
				var matchConditions = session.Query<IntervalRule>()
					.ToList()
					.SelectMany(x => x.Conditons.Select(s => new Tuple<string, string>(s, x.Group)));

                Contacts = Contacts.Select(x =>
                                               {
                                                   x.MemberOf = new List<string> {ListId};
	                                               x.DomainGroup = FindMatchingDomainGroupOrDefault(matchConditions, x);
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

		private string FindMatchingDomainGroupOrDefault(IEnumerable<Tuple<string, string>> matchedConditions, Contact row)
		{
			var group = matchedConditions.FirstOrDefault(x => row.Email.Contains(x.Item1));
			return group != null ? group.Item2 : _creativeFragmentSettings.DefaultGroup;
		}
    }
}