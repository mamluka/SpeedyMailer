using SpeedyMailer.Bridge.Model.Fragments;

namespace SpeedyMailer.Domain.DataAccess.Fragments
{
    public interface IFragmentRepository
    {
        void Add(EmailFragment fragment);
        EmailFragment PopFragment();
    }
}