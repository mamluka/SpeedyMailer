using System;
using MongoDB.Driver;
using Mongol;
using Quartz;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Drones.Commands;
using SpeedyMailer.Drones.Storage;

namespace SpeedyMailer.Drones.Tasks
{
	public class SendCreativePackagesWithIntervalTask : DynamiclyScheduledTaskWithData<SendCreativePackagesWithIntervalTask.Data>
	{
		public SendCreativePackagesWithIntervalTask(Action<Data> action, Action<SimpleScheduleBuilder> triggerBuilder)
			: base(action, triggerBuilder)
		{
		}

		public class Data : ScheduledTaskData
		{
			public int Interval { get; set; }
		}

		public override IJobDetail ConfigureJob()
		{
			return SimpleJob<Job>();
		}

		public class Job : JobBase<Data>, IJob
		{
			private readonly CreativePackagesStore _creativePackagesStore;
			private readonly SendCreativePackageCommand _sendCreativePackageCommand;

			public Job(SendCreativePackageCommand sendCreativePackageCommand, CreativePackagesStore creativePackagesStore)
			{
				_sendCreativePackageCommand = sendCreativePackageCommand;
				_creativePackagesStore = creativePackagesStore;
			}

			public void Execute(IJobExecutionContext context)
			{
				var data = GetData(context);
				_sendCreativePackageCommand.Package = _creativePackagesStore.GetPackageForInterval(data.Interval);
				_sendCreativePackageCommand.Execute();
			}
		}
	}
}