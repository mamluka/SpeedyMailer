using System.Collections.Generic;

namespace SpeedyMailer.Core.Domain.Drones
{
	public class SendingStatus
	{
		public long UnprocessedPackages { get; set; }
		public IList<Group> Groups { get; set; }

		public class Group
		{
			public string Name { get; set; }
			public long Total { get; set; }
		}
	}
}