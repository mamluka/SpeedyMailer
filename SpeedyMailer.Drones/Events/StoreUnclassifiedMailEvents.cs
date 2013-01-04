using System;
using System.Collections.Generic;
using System.Linq;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Evens;
using SpeedyMailer.Drones.Storage;

namespace SpeedyMailer.Drones.Events
{
	public class StoreUnclassifiedMailEvents : IHappendOn<AggregatedMailBounced>
	{
		private readonly OmniRecordManager _omniRecordManager;
		private CreativePackagesStore _creativePackagesStore;

		public StoreUnclassifiedMailEvents(OmniRecordManager omniRecordManager, CreativePackagesStore creativePackagesStore)
		{
			_creativePackagesStore = creativePackagesStore;
			_omniRecordManager = omniRecordManager;
		}

		public void Inspect(AggregatedMailBounced data)
		{
			StoreUnclassifiedMails(data);
		}

		private void StoreUnclassifiedMails<T>(AggregatedMailEvents<T> data) where T : IHasRecipient, IHasRelayMessage, IHasDomainGroup, IHasTime, IHasCreativeId, IHasClassification
		{
			var unclassified = data
				.MailEvents
				.Where(x => x.Type.Classification == Classification.NotClassified)
				.Select(x => new UnclassfiedMailEvent
					{
						CreativeId = x.CreativeId,
						Time = x.Time,
						DomainGroup = x.DomainGroup,
						Message = x.Message,
						Recipient = x.Recipient
					})
				.ToList();

			if (!unclassified.Any())
				return;

			var emails = unclassified.Select(x => x.Recipient);

			var creativePackages = _creativePackagesStore.GetByEmail(emails);

			creativePackages
				.ToList()
				.ForEach(x =>
					{
						x.TouchTime = DateTime.UtcNow;
						_creativePackagesStore.Save(x);
					});

			_omniRecordManager.BatchInsert(unclassified);
		}
	}
}