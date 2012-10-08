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
				.WithIdentity(Name,"ScheduledTasks")
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
		{ }

		public override IJobDetail GetJob()
		{
			var job = base.GetJob();
			job.JobDataMap.Add("data", JsonConvert.SerializeObject(TaskData));
			return job;
		}

		public ScheduledTaskData GetData()
		{
			return TaskData;
		}
	}

	public abstract class DynamiclyScheduledTaskWithData<T> : ScheduledTaskWithData<T> where T : ScheduledTaskData, new()
	{
		private readonly Action<SimpleScheduleBuilder> _triggerBuilder;

		protected DynamiclyScheduledTaskWithData(Action<T> action, Action<SimpleScheduleBuilder> triggerBuilder)
			: base(action)
		{
			_triggerBuilder = triggerBuilder;
		}

		public override ITrigger ConfigureTrigger()
		{
			return TriggerWithTimeCondition(_triggerBuilder);
		}
	}

	public abstract class DynamiclyScheduledTaskWithData : ScheduledTask
	{
		private readonly Action<SimpleScheduleBuilder> _triggerBuilder;

		protected DynamiclyScheduledTaskWithData(Action<SimpleScheduleBuilder> triggerBuilder)
		{
			_triggerBuilder = triggerBuilder;
		}

		protected DynamiclyScheduledTaskWithData()
		{ }

		public override ITrigger ConfigureTrigger()
		{
			return TriggerWithTimeCondition(_triggerBuilder);
		}
	}
}