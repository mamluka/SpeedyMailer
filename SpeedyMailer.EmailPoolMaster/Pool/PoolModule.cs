using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using Nancy.Responses;
using RestSharp;
using SpeedyMailer.Core.MailDrones;
using SpeedyMailer.EmailPoolMaster.MailDrones;
using Nancy.ModelBinding;

namespace SpeedyMailer.EmailPoolMaster.Pool
{
    public class PoolModule : NancyModule
    {


        public PoolModule(IMailDroneRepository mailDroneRepository,IMailDroneService mailDroneService) : base("/pool")
        {
            Post["/update"] = x =>
                                 {
                                     var drones = mailDroneRepository.SleepingDrones();
                                     foreach (var mailDrone in drones)
                                     {
                                         var status = mailDroneService.WakeUp(mailDrone);
                                         mailDrone.Status = status;
                                         mailDroneRepository.Update(mailDrone);

                                     }
                                     return HttpStatusCode.OK;
                                 };


            Get["/retrievefragment"] = x =>
                                          {
                                              var model =  this.Bind<FragmenRequest>();
                                              return Response.AsJson(model);
                                          };
        }

        
    }

    public class FragmenRequest
    {
        public MailDrone MailDrone { get; set; }
        public FragmentOporation FragmentOporation { get; set; }
    }

    public class FragmentOporation
    {
        public string FragmentId { get; set; }
        public FragmentOpotationType FragmentOpotationType { get; set; }
    }

    public enum FragmentOpotationType
    {
        SetAsCompleted
    }
}
