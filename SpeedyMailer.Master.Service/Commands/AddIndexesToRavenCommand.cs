using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client;
using SpeedyMailer.Core.Commands;

namespace SpeedyMailer.Master.Service.Commands
{
	public class AddIndexesToRavenCommand : Command
	{
		private readonly IDocumentStore _documentStore;

		public AddIndexesToRavenCommand(IDocumentStore documentStore)
		{
			_documentStore = documentStore;
		}

		public override void Execute()
		{
			Raven.Client.Indexes.IndexCreation.CreateIndexes(typeof(ServiceAssemblyMarker).Assembly, _documentStore);
		}
	}
}
