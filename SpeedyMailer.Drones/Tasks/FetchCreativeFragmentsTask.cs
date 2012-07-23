using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Mail;
using Quartz;
using RestSharp;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Tasks;
using System.Linq;
using SpeedyMailer.Drones.Commands;

namespace SpeedyMailer.Drones.Tasks
{
	public class FetchCreativeFragmentsTask : ScheduledTask
	{
		public override IJobDetail ConfigureJob()
		{
			return SimpleJob<Job>();
		}

		public override ITrigger ConfigureTrigger()
		{
			return TriggerWithTimeCondition(x => x.WithIntervalInMinutes(1).WithRepeatCount(1));
		}

		public class Job : IJob
		{
			private readonly Api _api;

			public Job(Api api,SendCreativePackageCommand sendCreativePackageCommand)
			{
				_api = api;
			}

			public void Execute(IJobExecutionContext context)
			{
				var response = _api.Call<ServiceEndpoints.FetchFragment, ServiceEndpoints.FetchFragment.Response>();

				var creativePackages = ToEmails(response.CreativeFragment);

				foreach (var package in creativePackages)
				{
					var mailMessage = new MailMessage
					                  	{
					                  		Body = package.Body,
					                  		Subject = package.Subject
					                  	};
					mailMessage.To.Add(package.To);
				}
			}

			private IList<CreativePackage> ToEmails(CreativeFragment creativeFragment)
			{
				return creativeFragment.Recipients.Select(x => new CreativePackage
																{
																	Body = GetBody(creativeFragment.Creative.Body,x),
																	Subject = creativeFragment.Creative.Subject,
																	To = x.Email,
																	

																}).ToList();
			}

			private string GetBody(string body, Contact contact)
			{
				return body;
			}
		}
	}
}