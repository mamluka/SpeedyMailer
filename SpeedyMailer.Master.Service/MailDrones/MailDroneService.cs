using RestSharp;
using SpeedyMailer.Bridge.Model.Drones;

namespace SpeedyMailer.Master.Service.MailDrones
{
    public class MailDroneService : IMailDroneService
    {
        private readonly IRestClient restClient;

        public MailDroneService(IRestClient restClient)
        {
            this.restClient = restClient;
        }


        public DroneStatus WakeUp(MailDrone mailDrone)
        {
            restClient.BaseUrl = mailDrone.BaseUri;

            var request = new RestRequest
                              {
                                  Method = Method.POST,
                                  Resource = mailDrone.WakeUpUri
                              };

            var result = restClient.Execute<DroneStatus>(request);

            if (result.ResponseStatus == ResponseStatus.Error)
            {
                return DroneStatus.ErrorOccured;
            }

            if (result.ResponseStatus == ResponseStatus.TimedOut)
            {
                return DroneStatus.NoCommunication;
            }

            return result.Data;
        }

    }
}