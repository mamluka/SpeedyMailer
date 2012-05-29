using System;
using Newtonsoft.Json;
using Quartz;

namespace SpeedyMailer.Drone.Tasks
{
	public abstract class ScheduledTask
	{
		public abstract string Name { get; }

		public abstract IJobDetail GetJob();
		public abstract ITrigger GetTrigger();
		public abstract ScheduledTaskData GetData();

		protected ITrigger TriggerWithTimeCondition(Action<SimpleScheduleBuilder> condition)
		{
			return TriggerBuilder.Create()
				.WithIdentity(Name+"Trigger")
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
				.UsingJobData("data",JsonConvert.SerializeObject(data))
				.Build();
		}
	}
}