using System.Collections.Generic;
using Nancy;
using Nancy.Json;
using Nancy.ModelBinding;
using SpeedyMailer.Bridge.Communication;
using SpeedyMailer.Bridge.Model.Drones;
using SpeedyMailer.Bridge.Model.Fragments;
using SpeedyMailer.Core.DataAccess.Drone;
using SpeedyMailer.Core.DataAccess.Fragments;
using SpeedyMailer.Master.Service.Emails;
using SpeedyMailer.Master.Service.MailDrones;

namespace SpeedyMailer.Master.Service.Api
{
    public class PoolModule : NancyModule
    {
        public PoolModule(IMailDroneRepository mailDroneRepository, IMailDroneService mailDroneService,
                          IPoolMailOporations emailOporations, IFragmentRepository fragmentRepository) : base("/pool")
        {
            Post["/update"] = x =>
                                  {
                                      List<MailDrone> drones = mailDroneRepository.CurrentlySleepingDrones();
                                      foreach (MailDrone mailDrone in drones)
                                      {
                                          DroneStatus status = mailDroneService.WakeUp(mailDrone);
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
                                               EmailFragment emailFragment = fragmentRepository.PopFragment();

                                               var fragmentResponse = new FragmentResponse
                                                                          {EmailFragment = emailFragment};

                                               if (emailFragment == null)
                                               {
                                                   MailDrone mailDrone = model.MailDrone;
                                                   mailDrone.Status = DroneStatus.Asleep;
                                                   mailDroneRepository.Update(mailDrone);

                                                   fragmentResponse.DroneSideOporations =
                                                       new List<DroneSideOporationBase> {new PutDroneToSleep()};
                                               }

                                               return Response.AsJson(fragmentResponse);
                                           };

        	Get["/test"] = x => Response.AsJs("Hello World!");
        }
    }
}