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
                                              var model =  this.Bind<FragmenPoolRequest>();
                                              return Response.AsJson(model);
                                          };
        }

        
    }

    public class FragmenPoolRequest
    {
        public MailDrone MailDrone { get; set; }
        public bool CompletedFragmentId { get; set; }
    }
}
