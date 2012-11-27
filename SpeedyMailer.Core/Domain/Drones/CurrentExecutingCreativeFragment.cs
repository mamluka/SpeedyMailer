using SpeedyMailer.Core.Domain.Master;

namespace SpeedyMailer.Core.Domain.Drones
{
	public class CurrentExecutingCreativeFragment
	{
		public string CreativeId { get; set; }
		public string Body { get; set; }
		public string ListId { get; set; }
		public string Subject { get; set; }
		public string UnsubscribeTemplate { get; set; }
		public string FromName { get; set; }
		public string FromAddressDomainPrefix { get; set; }
		public Service Service { get; set; }

		public string Id { get; set; }
	}
}