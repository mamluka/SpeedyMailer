using System;
using MongoDB.Driver;
using Mongol;
using NLog;
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
			public string Group { get; set; }
		}

		public override IJobDetail ConfigureJob()
		{
			return SimpleJob<Job>();
		}

		public class Job : JobBase<Data>, IJob
		{
			private readonly CreativePackagesStore _creativePackagesStore;
			private readonly SendCreativePackageCommand _sendCreativePackageCommand;
			private readonly Logger _logger;

			public Job(SendCreativePackageCommand sendCreativePackageCommand, CreativePackagesStore creativePackagesStore, Logger logger)
			{
				_logger = logger;
				_sendCreativePackageCommand = sendCreativePackageCommand;
				_creativePackagesStore = creativePackagesStore;
			}

			public void Execute(IJobExecutionContext context)
			{
				var data = GetData(context);
				var creativePackage = _creativePackagesStore.GetPackageForGroup(data.Group);

				if (StopSendingIfNoCreative(context, creativePackage)) return;

				if (creativePackage.RetryCount >= 3)
				{
					ProcessPackage(creativePackage);
					return;
				}

				try
				{
					_sendCreativePackageCommand.Package = creativePackage;
					_sendCreativePackageCommand.Execute();
				}
				catch (Exception ex)
				{
					_logger.ErrorException(string.Format("While sending to {0} a exception was thrown", creativePackage.To), ex);
				}

				ProcessPackage(creativePackage);
			}

			private bool StopSendingIfNoCreative(IJobExecutionContext context, CreativePackage creativePackage)
			{
				if (creativePackage == null)
				{
					this.Stop(context);
					return true;
				}
				return false;
			}

			private void ProcessPackage(CreativePackage creativePackage)
			{
				creativePackage.Processed = true;
				creativePackage.RetryCount++;
				_creativePackagesStore.Save(creativePackage);
			}
		}
	}
}