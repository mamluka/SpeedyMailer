namespace SpeedyMailer.Core.Emails
{
    public interface IEmailPoolService
    {
        AddEmailToPoolResults AddEmail(Email email);
    }
}