using System;
using System.Linq;
using NLog;
using Nancy;
using Quartz;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Drones.Commands;
using SpeedyMailer.Drones.Storage;

namespace SpeedyMailer.Drones.Modules
{
	public class AdminModule : NancyModule
	{
		public AdminModule(IScheduler scheduler, LogsStore logsStore, CreativePackagesStore creativePackagesStore,Logger logger,OmniRecordManager omniRecordManager)
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

			Post["/flush-unprocessed-packages"] = _ =>
				{
					var packages = creativePackagesStore.GetAll();
					packages.ToList().ForEach(x =>
						{
							x.Processed = true;
							logger.Info("write false for: {0}",x.To);
							creativePackagesStore.Save(x);
						});

					return Response.AsText("OK");
				};

			Get["/purge-blocking-rules"] = _ =>
				{
					omniRecordManager.RemoveSingle<GroupsAndIndividualDomainsSendingPolicies>();
					return "OK";
				};

		}
	}
}
