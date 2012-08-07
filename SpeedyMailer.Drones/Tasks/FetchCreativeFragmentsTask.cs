using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Mail;
using Quartz;
using RestSharp;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Domain.Master;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.Core.Tasks;
using System.Linq;
using SpeedyMailer.Core.Utilities;
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

		[DisallowConcurrentExecution]
		public class Job : IJob
		{
			private readonly Api _api;
			private readonly SendCreativePackageCommand _sendCreativePackageCommand;
			private readonly ICreativeBodySourceWeaver _creativeBodySourceWeaver;
			private readonly UrlBuilder _urlBuilder;

			public Job(Api api, SendCreativePackageCommand sendCreativePackageCommand, ICreativeBodySourceWeaver creativeBodySourceWeaver, UrlBuilder urlBuilder)
			{
				_urlBuilder = urlBuilder;
				_creativeBodySourceWeaver = creativeBodySourceWeaver;
				_sendCreativePackageCommand = sendCreativePackageCommand;
				_api = api;
			}

			public void Execute(IJobExecutionContext context)
			{
				try
				{
					var response = _api.Call<ServiceEndpoints.FetchFragment, ServiceEndpoints.FetchFragment.Response>();

					var creativePackages = ToCreativePackages(response.CreativeFragment);

					foreach (var package in creativePackages)
					{
						_sendCreativePackageCommand.Package = package;
						_sendCreativePackageCommand.Execute();
					}
				}
				catch (Exception ex)
				{
					Trace.WriteLine(ex.Message);
					throw;
				}
			}

			private IList<CreativePackage> ToCreativePackages(CreativeFragment creativeFragment)
			{
				var dealUrlBase = BuildDealUrl(creativeFragment.Service);
				return creativeFragment.Recipients.Select(x => new CreativePackage
																{
																	Body = PersonalizeBody(creativeFragment, dealUrlBase, x),
																	Subject = creativeFragment.Subject,
																	To = x.Email,
																}).ToList();
			}

			private string BuildDealUrl(Service service)
			{
				return string.Format("{0}/{1}", service.BaseUrl, service.DealsEndpoint);
			}

			private string PersonalizeBody(CreativeFragment fragment, string dealUrlBase, Contact contact)
			{
				var dealUrl = _urlBuilder
					.Base(dealUrlBase)
					.AddObject(GetDealUrl(fragment, contact))
					.AppendAsSlashes();

				return _creativeBodySourceWeaver.WeaveDeals(fragment.Body, dealUrl);
			}

			private static DealUrl GetDealUrl(CreativeFragment fragment, Contact contact)
			{
				return new DealUrl
				       	{
				       		CreativeId = fragment.CreativeId,
				       		ContactId = contact.Id
				       	};
			}
		}
	}
}