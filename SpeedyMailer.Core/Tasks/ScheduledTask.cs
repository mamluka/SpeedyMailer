using System;
using Newtonsoft.Json;
using Quartz;

namespace SpeedyMailer.Core.Tasks
{
	public abstract class ScheduledTask
	{
		public string Name { get { return GetType().FullName; } }
		public TimeSpan TaskTimeStartDelay { get; set; }

		public abstract IJobDetail ConfigureJob();
		public abstract ITrigger ConfigureTrigger();

		protected virtual ITrigger TriggerWithTimeCondition(Action<SimpleScheduleBuilder> condition)
		{
			return TriggerBuilder.Create()
				.WithIdentity(GetNamePrefix() + "Trigger")
				.WithSimpleSchedule(condition)
				.StartAt(new DateTimeOffset(DateTime.UtcNow + TaskTimeStartDelay))
				.Build();
		}

		public virtual string GetNamePrefix()
		{
			return Name;
		}

		protected virtual IJobDetail SimpleJob<T>() where T : IJob
		{
			return JobBuilder.Create<T>()
				.WithIdentity(GetNamePrefix() + "Job", "ScheduledTasks")
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

	public static class ScheduledTaskExtentions
	{
		public static ScheduledTask DelayFor(this ScheduledTask target, TimeSpan timeSpan)
		{
			target.TaskTimeStartDelay = timeSpan;
			return target;
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

		public override string GetNamePrefix()
		{
			return base.GetNamePrefix() + "_" + TaskData.GetHashCode() + "_";
		}

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

	public abstract class DynamiclyScheduledTask : ScheduledTask
	{
		private readonly Action<SimpleScheduleBuilder> _triggerBuilder;

		protected DynamiclyScheduledTask(Action<SimpleScheduleBuilder> triggerBuilder)
		{
			_triggerBuilder = triggerBuilder;
		}

		protected DynamiclyScheduledTask()
		{ }

		public override ITrigger ConfigureTrigger()
		{
			return TriggerWithTimeCondition(_triggerBuilder);
		}
	}
}