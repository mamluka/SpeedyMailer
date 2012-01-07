using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using Nancy.Responses;
using RestSharp;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.Core.MailDrones;
using SpeedyMailer.EmailPoolMaster.MailDrones;
using Nancy.ModelBinding;

namespace SpeedyMailer.EmailPoolMaster.Pool
{
    public class PoolModule : NancyModule
    {


        public PoolModule(IMailDroneRepository mailDroneRepository,IMailDroneService mailDroneService,IEMailOporations emailOporations,IEmailPool emailPool) : base("/pool")
        {
            Post["/update"] = x =>
                                 {
                                     var drones = mailDroneRepository.CurrentlySleepingDrones();
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
                                               var model = this.Bind<FragmenRequest>();
                                               if (model.PoolSideOporation != null)
                                               {
                                                   emailOporations.Preform(model.PoolSideOporation);
                                               }
                                               var emailFragment = emailPool.PopEmail();

                                               var fragmentResponse = new FragmentResponse
                                                                          {EmailFragment = emailFragment};

                                               if (emailFragment == null)
                                               {
                                                   var mailDrone = model.MailDrone;
                                                   mailDrone.Status = DroneStatus.Asleep;
                                                   mailDroneRepository.Update(mailDrone);

                                                   fragmentResponse.DroneSideOporations =
                                                       new List<DroneSideOporationBase>() {new PutDroneToSleep()};


                                               }

                                               return Response.AsJson(fragmentResponse);
                                           };
        }

        
    }
}
