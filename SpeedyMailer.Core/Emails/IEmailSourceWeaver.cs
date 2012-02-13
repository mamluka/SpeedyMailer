namespace SpeedyMailer.Core.Emails
{
    public interface IEmailSourceWeaver
    {
        string WeaveDeals(string bodySource, LeadIdentity dealObject);
        string WeaveUnsubscribeTemplate(string bodySource, string template, string unsubscribeLink);
        string WeaveDeals(string bodySource, string dealLink);
    }
}