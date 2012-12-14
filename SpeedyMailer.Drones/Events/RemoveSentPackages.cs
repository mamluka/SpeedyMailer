using System.Linq;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Evens;
using SpeedyMailer.Drones.Storage;

namespace SpeedyMailer.Drones.Events
{
	public class RemoveSentPackages : IHappendOn<AggregatedMailSent>
	{
		private readonly CreativePackagesStore _creativePackagesStore;

		public RemoveSentPackages(CreativePackagesStore creativePackagesStore)
		{
			_creativePackagesStore = creativePackagesStore;
		}

		public void Inspect(AggregatedMailSent data)
		{
			var emails = data
				.MailEvents
				.Select(x => x.Recipient)
				.ToList();

			var sentMailIds = _creativePackagesStore
				.GetByEmail(emails);

			sentMailIds.ForEach(x => _creativePackagesStore.DeleteById(x.Id));
		}
	}
}