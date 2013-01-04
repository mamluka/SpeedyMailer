using System.Collections.Generic;
using System.Linq;
using NLog;
using Quartz;
using RestSharp;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Core.Utilities;
using SpeedyMailer.Core.Utilities.Extentions;
using SpeedyMailer.Drones.Commands;
using SpeedyMailer.Drones.Storage;

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
			private readonly Framework _framework;
			private readonly CreativePackagesStore _creativePackagesStore;
			private readonly OmniRecordManager _omniRecordManager;
			private readonly MapToCreativePackageCommand _mapToCreativePackageCommand;
			private readonly Logger _logger;

			public Job(Framework framework,
					   Api api,
					   CreativePackagesStore creativePackagesStore,
					   OmniRecordManager omniRecordManager,
					   MapToCreativePackageCommand mapToCreativePackageCommand,
						Logger logger
				)
			{
				_logger = logger;
				_mapToCreativePackageCommand = mapToCreativePackageCommand;
				_omniRecordManager = omniRecordManager;
				_framework = framework;
				_creativePackagesStore = creativePackagesStore;
				_api = api;
			}

			public void Execute(IJobExecutionContext context)
			{
				var groupsSendingPolicies = _omniRecordManager.Load<GroupsAndIndividualDomainsSendingPolicies>() ?? new GroupsAndIndividualDomainsSendingPolicies();

				if (_creativePackagesStore.AreThereAnyNonProcessedPackages())
				{
					var packages = _creativePackagesStore.GetAll();
					var areThereActiveGroups = AreThereActiveGroups(packages, groupsSendingPolicies);

					if (!context.Scheduler.IsJobsRunning<SendCreativePackagesWithIntervalTask>() && areThereActiveGroups)
					{
						StartGroupSendingJobs(packages, groupsSendingPolicies);
						return;
					}

					if (areThereActiveGroups)
						return;
				}

				var creativeFragment = _api
					.Call<ServiceEndpoints.Creative.FetchFragment, CreativeFragment>();

				if (creativeFragment == null || _api.ResponseStatus != ResponseStatus.Completed)
					return;

				SaveCurrentCreativeToDealMap(creativeFragment);

				var recipiens = creativeFragment.Recipients;
				var creativePackages = recipiens.Select(x => ToCreativePackage(creativeFragment, x)).ToList();

				_creativePackagesStore.BatchInsert(creativePackages);

				StartGroupSendingJobs(creativePackages, groupsSendingPolicies);
			}

			private void StartGroupSendingJobs(IEnumerable<CreativePackage> recipiens, GroupsAndIndividualDomainsSendingPolicies sendingPolicies)
			{
				var groups = GetActiveGroups(recipiens, sendingPolicies);

				foreach (var domainGroup in groups)
				{

					_logger.Info("Start sending to group {0} with interval {1}", domainGroup.Group, domainGroup.Interval);

					_framework.StartTasks(new SendCreativePackagesWithIntervalTask(x =>
						{
							x.Group = domainGroup.Group;
						},
																				   x => x.WithIntervalInSeconds(domainGroup.Interval).RepeatForever()
											  ));
				}
			}

			private void SaveCurrentCreativeToDealMap(CreativeFragment creativeFragment)
			{
				_omniRecordManager.UpdateOrInsert(new CreativeToDealMap
													  {
														  Id = creativeFragment.CreativeId,
														  DealUrl = creativeFragment.DealUrl
													  });
			}

			private CreativePackage ToCreativePackage(CreativeFragment creativeFragment, Recipient recipient)
			{
				_mapToCreativePackageCommand.CreativeFragment = creativeFragment;
				_mapToCreativePackageCommand.Recipient = recipient;
				return _mapToCreativePackageCommand.Execute();
			}

			private bool AreThereActiveGroups(IEnumerable<CreativePackage> packages, GroupsAndIndividualDomainsSendingPolicies groupsAndIndividualDomainsSendingPolicies)
			{
				return GetActiveGroups(packages, groupsAndIndividualDomainsSendingPolicies).Any();
			}

			private static IEnumerable<PackageInfo> GetActiveGroups(IEnumerable<CreativePackage> recipiens, GroupsAndIndividualDomainsSendingPolicies andIndividualDomainsSendingPolicies)
			{
				return recipiens
					.GroupBy(x => x.Group)
					.Select(x => new PackageInfo { Group = x.Key, Interval = x.First().Interval, Count = x.Count() })
					.Where(x => !andIndividualDomainsSendingPolicies
									 .GroupSendingPolicies
									 .EmptyIfNull()
									 .ContainsKey(x.Group))
					.ToList();
			}
		}
	}
}