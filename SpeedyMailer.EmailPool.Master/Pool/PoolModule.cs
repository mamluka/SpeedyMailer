using System.Collections.Generic;
using Nancy;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.Core.MailDrones;
using Nancy.ModelBinding;
using SpeedyMailer.Core.Protocol;
using SpeedyMailer.EmailPool.Core.Emails;
using SpeedyMailer.EmailPool.Master.MailDrones;

namespace SpeedyMailer.EmailPool.Master.Pool
{
    public class PoolModule : NancyModule
    {


        public PoolModule(IMailDroneRepository mailDroneRepository,IMailDroneService mailDroneService,IMailOporations emailOporations,IEmailPoolService emailPoolService) : base("/pool")
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
                                               var emailFragment = emailPoolService.PopEmail();

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
