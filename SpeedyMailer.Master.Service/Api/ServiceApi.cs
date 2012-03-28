using System.Collections.Generic;
using Nancy;
using Nancy.Json;
using SpeedyMailer.Bridge.Communication;
using Nancy.ModelBinding;
using SpeedyMailer.Bridge.Model.Drones;
using SpeedyMailer.Domain.DataAccess.Drone;
using SpeedyMailer.Domain.DataAccess.Fragments;
using SpeedyMailer.Master.Service.Core.Emails;
using SpeedyMailer.Master.Service.MailDrones;

namespace SpeedyMailer.Master.Service.Api
{
    public class PoolModule : NancyModule
    {


        public PoolModule(IMailDroneRepository mailDroneRepository,IMailDroneService mailDroneService,IPoolMailOporations emailOporations,IFragmentRepository fragmentRepository) : base("/pool")
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
                                               JsonSettings.MaxJsonLength = 1000000000;

                                               var model = this.Bind<FragmenRequest>();
                                               if (model.PoolSideOporation != null)
                                               {
                                                   emailOporations.Preform(model.PoolSideOporation);
                                               }
                                               var emailFragment = fragmentRepository.PopFragment();

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
