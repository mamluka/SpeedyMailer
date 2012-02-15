namespace SpeedyMailer.Core.Emails
{
    public interface IFragmentRepository
    {
        void Add(EmailFragment fragment);
        EmailFragment PopFragment();
    }
}