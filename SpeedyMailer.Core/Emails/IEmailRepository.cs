namespace SpeedyMailer.Core.Emails
{
    public interface IEmailRepository
    {
        void Store(Email email);
    }
}