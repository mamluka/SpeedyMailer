using System;
using System.Diagnostics;
using System.Linq;
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

			Store.WaitForEntityToExist(resultId);

			var result = Store.Load<ComputationResult<string>>(resultId);

			result.Result.Should().Be("done");
		}

		[Test]
		public void Start_WhenTaskHasDynamicRepeatTime_ShouldUseTheRepeatTimeGiven()
		{
			var task = new TestDynamicScheduledTask(x => x.WithIntervalInSeconds(5).WithRepeatCount(4));

			_target.AddAndStart(task);

			Store.WaitForEntitiesToExist<ComputationResult<DateTime>>(4);

			var result = Store.Query<ComputationResult<DateTime>>().Select(x => x.Result).ToList();

			result.AssertTimeDifferenceInRange(5, 1);
		}

		[Test]
		public void Start_WhenTwoDynamicTasksAreStartedAtTheSameTime_ShouldEachUseTheGivenInterval()
		{
			var task1 = new TestDynamicScheduledTaskWithData(x => x.TestData = "first", x => x.WithIntervalInSeconds(3).WithRepeatCount(4));
			var task2 = new TestDynamicScheduledTaskWithData(x => x.TestData = "second", x => x.WithIntervalInSeconds(5).WithRepeatCount(4));

			_target.AddAndStart(new[] { task1, task2 });

			Store.WaitForEntitiesToExist<DataForTestingDynamicTasks>(10);

			var task1Results = Store.Query<DataForTestingDynamicTasks>(x => x.Data == "first").ToList();
			var task2Results = Store.Query<DataForTestingDynamicTasks>(x => x.Data == "second").ToList();

			task1Results.Select(x => x.Time).AssertTimeDifferenceInRange(3, 1);
			task2Results.Select(x => x.Time).AssertTimeDifferenceInRange(5, 1);
		}

		[Test]
		public void Start_WhenTaskWithDataHasDynamicRepeatTime_ShouldUseTheRepeatTimeGiven()
		{
			var task = new TestDynamicScheduledTaskWithData(x => x.TestData = "test data", x => x.WithIntervalInSeconds(5).WithRepeatCount(4));

			_target.AddAndStart(task);

			Store.WaitForEntitiesToExist<DataForTestingDynamicTasks>(4);

			var result = Store.Query<DataForTestingDynamicTasks>().Select(x => x.Time).ToList();

			result.AssertTimeDifferenceInRange(5, 1);
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

			Store.WaitForEntityToExist(resultId2);

			var result = Store.Load<ComputationResult<string>>(resultId2);
			result.Result.Should().Be("done");
		}

		[Test]
		public void ExecuteAndStart_WhenTheTaskTakesMoreTimeThenTheIntervalAndNoConcurrency_ShouldNotExecuteInParallel()
		{
			const string resultId = "result/1";
			var task = new ConcurrentScheduledTask(x => x.ResultId = resultId);

			_target.AddAndStart(task);

			Thread.Sleep(12 * 1000);

			var result = Store.Load<ComputationResult<int>>(resultId);

			result.Result.Should().Be(2);
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

	public class TestDynamicScheduledTask : DynamiclyScheduledTask
	{
		public TestDynamicScheduledTask(Action<SimpleScheduleBuilder> triggerBuilder)
			: base(triggerBuilder)
		{ }

		public override IJobDetail ConfigureJob()
		{
			return SimpleJob<Job>();
		}

		public class Job : IJob
		{
			private readonly Framework _framework;

			public Job(Framework framework)
			{
				_framework = framework;
			}

			public void Execute(IJobExecutionContext context)
			{
				_framework.Store(new ComputationResult<DateTime>
									{
										Result = DateTime.UtcNow
									});
			}
		}
	}

	public class TestDynamicScheduledTaskWithData : DynamiclyScheduledTaskWithData<TestDynamicScheduledTaskWithData.Data>
	{
		public TestDynamicScheduledTaskWithData(Action<Data> action, Action<SimpleScheduleBuilder> triggerBuilder)
			: base(action, triggerBuilder)
		{ }

		public class Data : ScheduledTaskData
		{
			public string TestData { get; set; }
		}

		public override IJobDetail ConfigureJob()
		{
			return SimpleJob<Job>();
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
				_framework.Store(new DataForTestingDynamicTasks
					                 {
						                 Time = DateTime.UtcNow,
						                 Data = data.TestData
					                 });
			}
		}
	}

	public class DataForTestingDynamicTasks
	{
		public DateTime Time { get; set; }
		public string Data { get; set; }
	}

	public class SecondTestScheduledTask : ScheduledTaskWithData<SecondTestScheduledTask.Data>
	{
		public SecondTestScheduledTask(Action<Data> action)
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
					Result = currentIndex + 1
				});

				Thread.Sleep(10 * 1000);
			}
		}
	}
}
