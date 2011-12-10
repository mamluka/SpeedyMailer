namespace SpeedyMailer.Core.Emails
{
    public interface IEmailPool
    {
        AddEmailToPoolResults AddEmail(Email email);
    }
}