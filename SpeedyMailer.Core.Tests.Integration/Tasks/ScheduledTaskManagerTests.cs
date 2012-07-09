using System;
using System.Threading;
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

		[Test]
		public void AddAndStart_WhenCalledTwiceWithSameTask_ShouldOverrideTheOldTask()
		{
			const string resultId1 = "result/1";
			const string resultId2 = "result/2";
			var firstTask = new TestScheduledTask(x =>
			{
				x.ResultId = resultId1;
			});

			_target.AddAndStart(firstTask);

			var secondTask = new TestScheduledTask(x =>
			{
				x.ResultId = resultId2;
			});

			_target.AddAndStart(secondTask);

			WaitForEntityToExist(resultId2);

			var result = Load<ComputationResult<string>>(resultId2);
			result.Result.Should().Be("done");
		}

		[Test]
		public void ExecuteAndStart_WhenTheTaskTakesMoreTimeThenTheIntervalAndNoConcurrency_ShouldNotExecuteInParallel()
		{
			const string resultId = "result/1";
			var task = new ConcurrentScheduledTask(x => x.ResultId = resultId);

			_target.AddAndStart(task);

			Thread.Sleep(12*1000);

			var result = Load<ComputationResult<int>>(resultId);

			result.Result.Should().Be(1);
		}
	}

	public class TestScheduledTask : ScheduledTaskWithData<TestScheduledTask.Data>
	{
		public TestScheduledTask(Action<Data> action)
			: base(action)
		{ }

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

	public class ConcurrentScheduledTask : ScheduledTaskWithData<ConcurrentScheduledTask.Data>
	{
		public ConcurrentScheduledTask(Action<Data> action)
			: base(action)
		{
		}

		public override IJobDetail ConfigureJob()
		{
			return SimpleJob<Job>();
		}

		public override ITrigger ConfigureTrigger()
		{
			return TriggerWithTimeCondition(x => x.WithIntervalInSeconds(3).WithRepeatCount(3));
		}

		public class Data : ScheduledTaskData
		{
			public string ResultId { get; set; }
		}

		[DisallowConcurrentExecution]
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
				var computation = _framework.Load<ComputationResult<int>>(data.ResultId);
				var currentIndex = computation == null ? 0 : computation.Result;

				_framework.Store(new ComputationResult<int>
				{
					Id = data.ResultId,
					Result = currentIndex+1
				});

				Thread.Sleep(10*1000);
			}
		}
	}
}
