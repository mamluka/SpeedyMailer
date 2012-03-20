namespace SpeedyMailer.Core.Emails
{
    public interface IEmailSourceWeaver
    {
        string WeaveUnsubscribeTemplate(string bodySource, string template, string unsubscribeLink);
        string WeaveDeals(string bodySource, string dealLink);
    }
}