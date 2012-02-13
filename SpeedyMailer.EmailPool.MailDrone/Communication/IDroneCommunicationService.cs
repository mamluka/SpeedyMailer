using SpeedyMailer.Core.Protocol;

namespace SpeedyMailer.EmailPool.MailDrone.Communication
{
    public interface IDroneCommunicationService
    {
        FragmentResponse RetrieveFragment();
    }
}