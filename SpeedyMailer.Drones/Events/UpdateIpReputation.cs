using System;
using System.Collections.Generic;
using System.Linq;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Evens;
using SpeedyMailer.Drones.Storage;

namespace SpeedyMailer.Drones.Events
{
	public class UpdateIpReputation : IHappendOn<BlockingGroups>, IHappendOn<ResumingGroups>
	{
		private readonly OmniRecordManager _omniRecordManager;

		public UpdateIpReputation(OmniRecordManager omniRecordManager)
		{
			_omniRecordManager = omniRecordManager;
		}

		public void Inspect(BlockingGroups data)
		{
			UpdateReputation(data.Groups, x => x.BlockingHistory);
		}

		public void Inspect(ResumingGroups data)
		{
			UpdateReputation(data.Groups, x => x.ResumingHistory);
		}

		private void UpdateReputation(IEnumerable<string> groups, Func<IpReputation, IDictionary<string, List<DateTime>>> selector)
		{
			var ipReputation = _omniRecordManager.GetSingle<IpReputation>() ?? NewIpReputation();

			groups
			.ToList()
			.ForEach(x =>
							 {
								 var dictionary = selector(ipReputation);
								 var containsKey = dictionary.ContainsKey(x);

								 if (containsKey)
								 {
									 dictionary[x].Add(DateTime.UtcNow);
								 }
								 else
								 {
									 dictionary.Add(x, new List<DateTime> { DateTime.UtcNow });
								 }
							 });

			_omniRecordManager.UpdateOrInsert(ipReputation);
		}

		private static IpReputation NewIpReputation()
		{
			return new IpReputation
					   {
						   BlockingHistory = new Dictionary<string, List<DateTime>>(),
						   ResumingHistory = new Dictionary<string, List<DateTime>>()
					   };
		}
	}
}