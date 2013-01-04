using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Utilities.Extentions;
using SpeedyMailer.Drones.Storage;

namespace SpeedyMailer.Drones.Commands
{
	public class MarkDomainsAsProcessedCommand : Command
	{
		private readonly CreativePackagesStore _creativePackagesStore;
		private readonly Logger _logger;

		public IList<string> Domains { get; set; }
		public string LoggingLine { get; set; }

		public MarkDomainsAsProcessedCommand(CreativePackagesStore creativePackagesStore, Logger logger)
		{
			_logger = logger;
			_creativePackagesStore = creativePackagesStore;
		}

		public override void Execute()
		{
			var packages = _creativePackagesStore.GetByDomains(Domains);

			packages
				.ToList()
				.ForEach(x =>
				{
					x.Processed = true;
					_creativePackagesStore.Save(x);
				});

			_logger.Info(LoggingLine, Domains.Commafy(), packages.Select(x => x.To).Commafy());


		}
	}
}
