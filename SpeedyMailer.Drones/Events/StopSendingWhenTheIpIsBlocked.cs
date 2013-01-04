using System.Linq;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Evens;
using SpeedyMailer.Core.Utilities.Extentions;
using SpeedyMailer.Drones.Storage;

namespace SpeedyMailer.Drones.Events
{
	public class StopSendingWhenTheIpIsBlocked : IHappendOn<AggregatedMailBounced>
	{
		private CreativePackagesStore _creativePackagesStore;

		public StopSendingWhenTheIpIsBlocked(CreativePackagesStore creativePackagesStore)
		{
			_creativePackagesStore = creativePackagesStore;
		}

		public void Inspect(AggregatedMailBounced data)
		{
			var domainsToStop = data
				.MailEvents
				.Where(x => x.Classification.Type == Classification.IpBlocking)
				.Where(x => x.Domain.HasValue())
				.Select(x => x.Domain)
				.ToList();

			var packages = _creativePackagesStore.GetByDomains(domainsToStop);

			packages
				.ToList()
				.ForEach(x =>
					{
						x.Processed = true;
						_creativePackagesStore.Save(x);
					});


		}
	}
}