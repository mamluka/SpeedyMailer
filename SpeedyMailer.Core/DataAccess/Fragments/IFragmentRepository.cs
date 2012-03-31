using SpeedyMailer.Bridge.Model.Fragments;

namespace SpeedyMailer.Core.DataAccess.Fragments
{
    public interface IFragmentRepository
    {
        void Add(EmailFragment fragment);
        EmailFragment PopFragment();
    }
}