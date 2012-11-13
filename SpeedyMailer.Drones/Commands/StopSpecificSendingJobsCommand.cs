using System.Linq;
using Newtonsoft.Json;
using Quartz;
using Quartz.Impl.Matchers;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Drones.Tasks;

namespace SpeedyMailer.Drones.Commands
{
	public class StopSpecificSendingJobsCommand : Command
	{
		private readonly IScheduler _scheduler;

		public string Group { get; set; }

		public StopSpecificSendingJobsCommand(IScheduler scheduler)
		{
			_scheduler = scheduler;
		}

		public override void Execute()
		{
			var groups = _scheduler.GetJobGroupNames();
			var jobKeys = groups.SelectMany(x => _scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(x)));

			var jobsToStop = jobKeys
				.Where(IsBelongsToGroup)
				.ToList();

			_scheduler.DeleteJobs(jobsToStop);
		}

		private bool IsBelongsToGroup(JobKey jobKey)
		{
			var jobDetail = _scheduler.GetJobDetail(jobKey);
			if (jobDetail.JobDataMap == null || !jobDetail.JobDataMap.Contains("data"))
				return false;
			
			var data = JsonConvert.DeserializeObject<SendCreativePackagesWithIntervalTask.Data>((string) jobDetail.JobDataMap["data"]);
			return data.Group == Group;
		}
	}
}