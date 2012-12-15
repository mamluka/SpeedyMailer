using System;
using System.Collections.Generic;
using System.Linq;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Evens;
using SpeedyMailer.Drones.Commands;
using SpeedyMailer.Drones.Storage;

namespace SpeedyMailer.Drones.Events
{
	public class UpdateIpReputation : IHappendOn<AggregatedMailBounced>, IHappendOn<AggregatedMailDeferred>
	{
		private ClassifyNonDeliveredMailCommand _classifyNonDeliveredMailCommand;
		private OmniRecordManager _omniRecordManager;

		public UpdateIpReputation(ClassifyNonDeliveredMailCommand classifyNonDeliveredMailCommand, OmniRecordManager omniRecordManager)
		{
			_omniRecordManager = omniRecordManager;
			_classifyNonDeliveredMailCommand = classifyNonDeliveredMailCommand;
		}

		public void Inspect(AggregatedMailBounced data)
		{
			var ipReputation = _omniRecordManager.GetSingle<IpReputation>() ?? new IpReputation() { GroupReputation = new Dictionary<string, List<DateTime>>() };
			data.MailEvents.Select(x =>
				{
					_classifyNonDeliveredMailCommand.Message = x.Message;
					var classfication = _classifyNonDeliveredMailCommand.Execute();
					return new { Clasification = classfication.BounceType, x.DomainGroup };
				})
				.Where(x => x.Clasification == BounceType.Blocked)
				.ToList()
				.ForEach(x =>
					{
						var newKey = ipReputation.GroupReputation.ContainsKey(x.DomainGroup);
						if (newKey)
						{
							ipReputation.GroupReputation[x.DomainGroup].Add(DateTime.UtcNow);
						}
						else
						{
							ipReputation.GroupReputation.Add(x.DomainGroup, new List<DateTime> { DateTime.UtcNow });
						}

					});

			_omniRecordManager.UpdateOrInsert(ipReputation);
		}

		public void Inspect(AggregatedMailDeferred data)
		{

		}
	}
}