using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Quartz;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Domain.Master;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Core.Utilities;
using SpeedyMailer.Core.Utilities.Extentions;
using SpeedyMailer.Drones.Storage;
using Template = Antlr4.StringTemplate.Template;


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
			return TriggerWithTimeCondition(x => x.WithIntervalInMinutes(1).RepeatForever());
		}

		[DisallowConcurrentExecution]
		public class Job : IJob
		{
			private readonly Api _api;
			private readonly ICreativeBodySourceWeaver _creativeBodySourceWeaver;
			private readonly UrlBuilder _urlBuilder;
			private readonly Framework _framework;
			private readonly CreativePackagesStore _creativePackagesStore;
			private readonly OmniRecordManager _omniRecordManager;

			public Job(Framework framework,
				Api api,
				ICreativeBodySourceWeaver creativeBodySourceWeaver,
				UrlBuilder urlBuilder,
				CreativePackagesStore creativePackagesStore,
				OmniRecordManager omniRecordManager)
			{
				_omniRecordManager = omniRecordManager;
				_framework = framework;
				_creativePackagesStore = creativePackagesStore;
				_urlBuilder = urlBuilder;
				_creativeBodySourceWeaver = creativeBodySourceWeaver;
				_api = api;
			}

			public void Execute(IJobExecutionContext context)
			{
				var groupsSendingPolicies = _omniRecordManager.GetSingle<GroupsSendingPolicies>() ?? new GroupsSendingPolicies();

				if (_creativePackagesStore.AreThereAnyPackages())
				{
					var packages = _creativePackagesStore.GetAll();

					if (!context.Scheduler.IsJobsRunning<SendCreativePackagesWithIntervalTask>() && AreThereActiveGroups(packages, groupsSendingPolicies))
					{
						StartGroupSendingJobs(packages, groupsSendingPolicies);

						return;
					}
				}

				var creativeFragment = _api
					.Call<ServiceEndpoints.Creative.FetchFragment, CreativeFragment>();

				if (creativeFragment == null)
					return;

				var recipiens = creativeFragment.Recipients;
				var creativePackages = recipiens.Select(x => ToPackage(x, creativeFragment)).ToList();

				_creativePackagesStore.BatchInsert(creativePackages);

				StartGroupSendingJobs(creativePackages, groupsSendingPolicies);
			}

			private bool AreThereActiveGroups(IEnumerable<CreativePackage> packages, GroupsSendingPolicies groupsSendingPolicies)
			{
				return GetActiveGroups(packages, groupsSendingPolicies).Any();
			}

			private void StartGroupSendingJobs(IEnumerable<CreativePackage> recipiens, GroupsSendingPolicies sendingPolicies)
			{
				var groups = GetActiveGroups(recipiens, sendingPolicies);

				foreach (var group in groups)
				{
					_framework.StartTasks(new SendCreativePackagesWithIntervalTask(x =>
																					   {
																						   x.Group = @group.Group;
																					   },
																				   x => x.WithIntervalInSeconds(@group.Interval).WithRepeatCount(@group.Count)
											  ));
				}
			}

			private static IEnumerable<PackageInfo> GetActiveGroups(IEnumerable<CreativePackage> recipiens, GroupsSendingPolicies sendingPolicies)
			{
				return recipiens
					.GroupBy(x => x.Group)
					.Select(x => new PackageInfo { Group = x.Key, Interval = x.First().Interval, Count = x.Count() })
					.Where(x => !sendingPolicies
									 .GroupSendingPolicies
									 .EmptyIfNull()
									 .ContainsKey(x.Group))
					.ToList();
			}

			private CreativePackage ToPackage(Recipient recipient, CreativeFragment creativeFragment)
			{
				return new CreativePackage
						{
							Subject = creativeFragment.Subject,
							Body = PersonalizeBody(creativeFragment, recipient),
							To = recipient.Email,
							Group = recipient.Group,
							FromName = creativeFragment.FromName,
							FromAddressDomainPrefix = creativeFragment.FromAddressDomainPrefix,
							Interval = recipient.Interval
						};
			}

			private string ServiceEndpoint(Service service, Func<Service, string> endpointSelector)
			{
				return string.Format("{0}/{1}", service.BaseUrl, endpointSelector(service));
			}

			private string PersonalizeBody(CreativeFragment fragment, Recipient contact)
			{
				var service = fragment.Service;

				var dealUrl = _urlBuilder
					.Base(ServiceEndpoint(service, x => x.DealsEndpoint))
					.AddObject(GetDealUrlData(fragment, contact))
					.AppendAsSlashes();

				var unsubsribeUrl = _urlBuilder
					.Base(ServiceEndpoint(service, x => x.UnsubscribeEndpoint))
					.AddObject(GetDealUrlData(fragment, contact))
					.AppendAsSlashes();

				var bodyTemplateEngine = new Template(fragment.Body, '^', '^');
				bodyTemplateEngine.Add("email", contact.Email);

				var body = bodyTemplateEngine.Render();

				var weavedBody = _creativeBodySourceWeaver.WeaveDeals(body, dealUrl);

				var unsubscribeTemplateEngine = new Template(fragment.UnsubscribeTemplate, '^', '^');
				unsubscribeTemplateEngine.Add("url", unsubsribeUrl);

				var template = unsubscribeTemplateEngine.Render();

				return weavedBody + template;
			}

			private static DealUrlData GetDealUrlData(CreativeFragment fragment, Recipient contact)
			{
				return new DealUrlData
						{
							CreativeId = fragment.CreativeId,
							ContactId = contact.ContactId
						};
			}
		}

		internal class PackageInfo
		{
			public string Group { get; set; }

			public int Interval { get; set; }

			public int Count { get; set; }
		}
	}


}