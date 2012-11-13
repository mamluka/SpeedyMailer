﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Nancy;
using Quartz;
using SpeedyMailer.Core.Tasks;

namespace SpeedyMailer.Drones.Modules
{
	public class AdminModule : NancyModule
	{
		public AdminModule(IScheduler scheduler)
			: base("/admin")
		{
			Trace.WriteLine("drone admin module laoded");

			Post["/tasks/fire-exiting/{job}"] = x =>
				                                    {
														scheduler.TriggerTaskByClassName((string)x.job);
					                                    return Response.AsText("OK");
				                                    };
		}
	}
}
