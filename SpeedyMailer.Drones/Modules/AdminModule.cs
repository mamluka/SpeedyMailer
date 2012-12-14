using System;
using System.Linq;
using Nancy;
using Quartz;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Drones.Commands;
using SpeedyMailer.Drones.Storage;

namespace SpeedyMailer.Drones.Modules
{
	public class AdminModule : NancyModule
	{
		public AdminModule(IScheduler scheduler, LogsStore logsStore, ParseLogsCommand parseLogsCommand)
			: base("/admin")
		{

			Get["/hello"] = x => Response.AsText("OK");

			Get["/fire-task/{job}"] = x =>
													{
														scheduler.TriggerTaskByClassName((string)x.job);
														return Response.AsText("OK");
													};

			Get["/postfix-logs"] = x =>
									   {
										   var logs = logsStore.GetAllLogs();
										   var lines = logs.Select(entry => string.Format("{0} {1}", entry.time.ToLongTimeString(), entry.msg)).ToList();
										   return Response.AsText(string.Join(Environment.NewLine, lines));
									   };

			Get["/jobs"] = x =>
				{
					var tasks = scheduler.GetCurrentJobs();
					var data = tasks.Select(jobKey => new { Data = scheduler.GetJobDetail(jobKey).JobDataMap, Job = jobKey.Name }).ToList();

					return Response.AsJson(data);
				};
		}
	}
}
