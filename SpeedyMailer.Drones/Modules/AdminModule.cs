using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Nancy;
using Quartz;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Drones.Commands;
using SpeedyMailer.Drones.Storage;

namespace SpeedyMailer.Drones.Modules
{
	public class AdminModule : NancyModule
	{
		public AdminModule(IScheduler scheduler,LogsStore logsStore,SendCreativePackageCommand sendCreativePackageCommand)
			: base("/admin")
		{

			Get["/hello"] = x => Response.AsText("OK");

			Get["/fire-task/{job}"] = x =>
													{
														scheduler.TriggerTaskByClassName((string)x.job);
														return Response.AsText("OK");
													};

			Get["/shutdown"] = x =>
				                   {
					                   var jobs = scheduler.GetCurrentJobs();
					                   scheduler.DeleteJobs(jobs);

									   scheduler.Shutdown();

					                   while (!scheduler.IsShutdown)
					                   {
						                   Thread.Sleep(500);
					                   }

									   Environment.Exit(0);

									   return Response.AsText("OK");
				                   };

			Get["/raw-postfix-logs"] = x => Response.AsJson(logsStore.GetAllLogs());

			Get["/postfix-logs"] = x =>
				                       {
					                       var logs = logsStore.GetAllLogs();
					                       var lines = logs.Select(entry => string.Format("{0} {1}", entry.time.ToLongTimeString(), entry.msg)).ToList();
					                       return Response.AsText(string.Join(Environment.NewLine, lines));
				                       };

			Get["/send-test-email"] = x =>
				                          {

					                          sendCreativePackageCommand.Package = new CreativePackage
						                                                               {
																						   Body = "testing: " + Guid.NewGuid(),
																						   FromName = "testing",
																						   Subject = DateTime.UtcNow.ToLongTimeString() + " testing subject",
																						   To = Request.Query["to"],
																						   FromAddressDomainPrefix = "test"
						                                                               };
					                          sendCreativePackageCommand.FromAddressDomainPrefix = "test";
					                          sendCreativePackageCommand.FromName = "testing";

											  sendCreativePackageCommand.Execute();

					                          return Response.AsText("OK");
				                          };
		}
	}
}
