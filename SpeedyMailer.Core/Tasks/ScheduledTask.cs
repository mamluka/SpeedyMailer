using System;
using Newtonsoft.Json;
using Quartz;

namespace SpeedyMailer.Core.Tasks
{
	public abstract class ScheduledTask
	{
		public string Name { get { return GetType().FullName; } }

		public abstract IJobDetail ConfigureJob();
		public abstract ITrigger ConfigureTrigger();

		protected ITrigger TriggerWithTimeCondition(Action<SimpleScheduleBuilder> condition)
		{
			return TriggerBuilder.Create()
				.WithIdentity(Name + "Trigger")
				.WithSimpleSchedule(condition)
				.StartNow()
				.Build();
		}

		protected IJobDetail SimpleJob<T>() where T : IJob
		{
			return JobBuilder.Create<T>()
				.WithIdentity(Name)
				.RequestRecovery()
				.Build();
		}

		public virtual IJobDetail GetJob()
		{
			return ConfigureJob();
		}

		public ITrigger GetTrigger()
		{
			return ConfigureTrigger();
		}

	}

	public abstract class ScheduledTaskWithData<T> : ScheduledTask where T : ScheduledTaskData, new()
	{
		protected readonly T TaskData;

		protected ScheduledTaskWithData(Action<T> action)
		{
			TaskData = new T();
			action.Invoke(TaskData);
		}

		protected ScheduledTaskWithData()
		{
			
		}

		public override IJobDetail GetJob()
		{
			var job =  base.GetJob();
			job.JobDataMap.Add("data", JsonConvert.SerializeObject(TaskData));
			return job;
		}

		public ScheduledTaskData GetData()
		{
			return TaskData;
		}
	}
}