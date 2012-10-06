using System.Collections.Generic;
using Raven.Client;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Rules;

namespace SpeedyMailer.Master.Service.Commands
{
	public class AddIntervalRulesCommand : Command
	{
		private readonly IDocumentStore _documentStore;

		public IEnumerable<IntervalRule> Rules { get; set; }

		public AddIntervalRulesCommand(IDocumentStore documentStore)
		{
			_documentStore = documentStore;
		}

		public override void Execute()
		{
			using (var session = _documentStore.OpenSession())
			{
				foreach (var intervalRule in Rules)
				{
					session.Store(intervalRule);
				}

				session.SaveChanges();
			}
		}
	}
}