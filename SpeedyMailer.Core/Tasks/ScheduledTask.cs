using System;
using Newtonsoft.Json;
using Quartz;

namespace SpeedyMailer.Core.Tasks
{
	public abstract class ScheduledTask
	{
		public string Name { get { return GetType().FullName; } }

		public abstract IJobDetail GetJob();
		public abstract ITrigger GetTrigger();

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

		protected IJobDetail SimpleJob<T>(ScheduledTaskData data) where T : IJob
		{
			return JobBuilder.Create<T>()
				.WithIdentity(Name)
				.RequestRecovery()
				.UsingJobData("data", JsonConvert.SerializeObject(data))
				.Build();
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

		public ScheduledTaskData GetData()
		{
			return TaskData;
		}
	}
}