﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Nancy;
using Quartz;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Drones.Storage;

namespace SpeedyMailer.Drones.Modules
{
	public class AdminModule : NancyModule
	{
		public AdminModule(IScheduler scheduler,LogsStore logsStore)
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
		}
	}
}
