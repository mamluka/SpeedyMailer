namespace SpeedyMailer.Core.Emails
{
    public interface ICreativeBodySourceWeaver
    {
        string WeaveUnsubscribeTemplate(string bodySource, string template, string unsubscribeLink);
        string WeaveDeals(string bodySource, string dealLink);
    }
}