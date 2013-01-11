using System.Linq;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Evens;
using SpeedyMailer.Drones.Commands;
using SpeedyMailer.Drones.Storage;

namespace SpeedyMailer.Drones.Events
{
	public class ResendTryAgainProtectedContacts : IHappendOn<AggregatedMailBounced>
	{
		private readonly CreativePackagesStore _creativePackagesStore;
		private readonly SendCreativePackageCommand _sendCreativePackageCommand;

		public ResendTryAgainProtectedContacts(CreativePackagesStore creativePackagesStore, SendCreativePackageCommand sendCreativePackageCommand)
		{
			_sendCreativePackageCommand = sendCreativePackageCommand;
			_creativePackagesStore = creativePackagesStore;
		}

		public void Inspect(AggregatedMailBounced data)
		{
			var tryAgain = data.MailEvents
				.Where(x => x.Classification.Type == Classification.TryAgain)
				.Select(x => x.Recipient)
				.ToList();

			var packages = _creativePackagesStore.GetByEmail(tryAgain);

			packages.ForEach(x =>
				{
					_sendCreativePackageCommand.Package = x;
					_sendCreativePackageCommand.Execute();
				});
		}
	}
}