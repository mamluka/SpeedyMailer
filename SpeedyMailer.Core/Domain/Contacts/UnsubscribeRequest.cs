namespace SpeedyMailer.Core.Domain.Contacts
{
	public class UnsubscribeRequest
	{
		public string Id { get; set; }
		public string CreativeId { get; set; }
		public string ContactId { get; set; }
	}
}