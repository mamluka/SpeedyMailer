using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NLog;
using Raven.Client;
using Raven.Client.Exceptions;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Rules;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Core.Utilities.Extentions;
using SpeedyMailer.Master.Service.Tasks;

namespace SpeedyMailer.Master.Service.Commands
{
	public class AddContactsCommand : Command<long>
	{
		private readonly IDocumentStore _documentStore;
		private readonly CreativeFragmentSettings _creativeFragmentSettings;
		private Logger _logger;
		public IEnumerable<Contact> Contacts { get; set; }
		public string ListId { get; set; }

		public AddContactsCommand(IDocumentStore documentStore, CreativeFragmentSettings creativeFragmentSettings, Logger logger)
		{
			_logger = logger;
			_creativeFragmentSettings = creativeFragmentSettings;
			_documentStore = documentStore;
		}

		public override long Execute()
		{
			const int counter = 0;
			using (var session = _documentStore.OpenSession())
			{
				var matchConditions = session.Query<IntervalRule>()
					.ToList()
					.SelectMany(x => x.Conditons.Select(s => new Tuple<string, string>(s, x.Group)));

				Contacts = Contacts.Select(x =>
											   {
												   x.MemberOf = new List<string> { ListId };
												   x.DomainGroup = FindMatchingDomainGroupOrDefault(matchConditions, x);
												   return x;
											   }).ToList();

				_logger.Info("Found {0} contacts", Contacts.Count());

				var chunks = Contacts
					.Clump(3000)
					.ToList();

				_logger.Info("Created {0} chunks", chunks.Count);

				foreach (var chunk in chunks)
				{
					chunk
						.ToList()
						.ForEach(session.Store);

					session.SaveChanges();
					Thread.Sleep(500);
				}

				return counter;
			}
		}

		private string FindMatchingDomainGroupOrDefault(IEnumerable<Tuple<string, string>> matchedConditions, Contact row)
		{
			var group = matchedConditions.FirstOrDefault(x => row.Email.ToLower().Contains(x.Item1.ToLower()));
			return group != null ? group.Item2 : _creativeFragmentSettings.DefaultGroup;
		}
	}
}