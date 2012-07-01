using System;
using FluentAssertions;
using NUnit.Framework;
using Quartz;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Core.Utilities;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Core.IntegrationTests.Tasks
{
	[TestFixture]
	public class ScheduledTaskManagerTests : IntegrationTestBase
	{
		private IScheduledTaskManager _target;

		public override void ExtraSetup()
		{
			_target = MasterResolve<IScheduledTaskManager>();
		}

		[Test]
		public void Start_WhenGivenAScheduledTask_ShouldScheduleAndStartTheTask()
		{
			const string resultId = "result/1";
			var task = new TestScheduledTask(x =>
			                                 	{

			                                 		x.ResultId = resultId;
			                                 	});

			_target.AddAndStart(task);

			WaitForEntityToExist(resultId);

			var result = Load<ComputationResult<string>>(resultId);

			result.Result.Should().Be("done");


		}
	}

	public class TestScheduledTask : ScheduledTaskWithData<TestScheduledTask.Data>
	{
		public TestScheduledTask(Action<Data> action)
			: base(action)
		{
		}

		public override IJobDetail ConfigureJob()
		{
			return SimpleJob<Job>();
		}

		public override ITrigger ConfigureTrigger()
		{
			return TriggerWithTimeCondition(x => x.WithIntervalInSeconds(5).WithRepeatCount(1));
		}

		public class Data : ScheduledTaskData
		{
			public string ResultId { get; set; }
		}

		public class Job : JobBase<Data>, IJob
		{
			private readonly Framework _framework;

			public Job(Framework framework)
			{
				_framework = framework;
			}

			public void Execute(IJobExecutionContext context)
			{
				var data = GetData(context);

				_framework.Store(new ComputationResult<string>
				                 	{
										Id = data.ResultId,
				                 		Result = "done"
				                 	});
			}
		}
	}
}
