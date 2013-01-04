namespace SpeedyMailer.Core.Domain.Mail
{
	public interface IHasClassification
	{
		MailClassfication Classification { get; set; }
	}
}