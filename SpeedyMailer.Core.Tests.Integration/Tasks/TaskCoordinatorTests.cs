using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Core.Tasks.Testing;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Core.IntegrationTests.Tasks
{
	[TestFixture]
	public class TaskCoordinatorTests : IntegrationTestBase
	{
		private ITaskCoordinator _target;
		private ITaskManager _taskManager;

		public override void ExtraSetup()
		{
			_taskManager = MasterResolve<ITaskManager>();

			_target = MasterResolve<ITaskCoordinator>();
		}

		[Test]
		public void BeginExecuting_WhenCalled_ShouldSpawnAThreadToPerformJobs()
		{
			const string resultId = "addition/1";
			var task1 = new AdditionTask
							{
								FirstNubmer = 2,
								SecondNumber = 2,
								ResultId = resultId
							};

			_taskManager.Save(task1);

			_target.BeginExecuting();

			Store.WaitForEntityToExist(resultId);

			var result = Store.Load<ComputationResult<int>>(resultId);
			result.Result.Should().Be(4);
		}

		[Test]
		public void BeginExecuting_WhenJobAlreadyRunning_ShouldDoNothing()
		{
			var longTask = new LongTask
			               	{
			               		Seconds = 30
			               	};

			const string resultId = "addition/1";
			var task1 = new AdditionTask
			            	{
			            		FirstNubmer = 2,
			            		SecondNumber = 2,
			            		ResultId = resultId
			            	};

			_taskManager.Save(longTask);
			_target.BeginExecuting();

			_taskManager.Save(task1);
			_target.BeginExecuting();

			Store.WaitForEntityToExist(resultId,10);

			var result = Store.Load<ComputationResult<int>>(resultId);
			result.Should().BeNull();
		}

		[Test]
		public void BeginExecuting_WhenAJobWasTriggeredAndWasOverAndBeginExecutingWasCalledAgain_ShouldStartNewJob()
		{
			const string resultId1 = "addition/1";
			const string resultId2 = "addition/2";
			var task1 = new AdditionTask
							{
								FirstNubmer = 2,
								SecondNumber = 2,
								ResultId = resultId1
							};

			var task2 = new AdditionTask
			            	{
			            		FirstNubmer = 3,
			            		SecondNumber =3,
			            		ResultId = resultId2
			            	};

			_taskManager.Save(task1);
			_target.BeginExecuting();

			Store.WaitForEntityToExist(resultId1, 5);

			_taskManager.Save(task2);
			_target.BeginExecuting();

			Store.WaitForEntityToExist(resultId2, 10);

			var result1 = Store.Load<ComputationResult<int>>(resultId1);
			var result2 = Store.Load<ComputationResult<int>>(resultId2);

			result1.Result.Should().Be(4);
			result2.Result.Should().Be(6);
		}

		public void BeginExecuting_WhenMaxNumberOfParallelJobsIsExecutong_ShouldNotStartANewJpb()
		{
			var task1 = new LongTask { Seconds = 10 };
			var task2 = new LongTask { Seconds = 10 };
			var task3 = new LongTask { Seconds = 10 };

			const string resultId = "addition/1";
			var task4 = new AdditionTask
			{
				FirstNubmer = 2,
				SecondNumber = 2,
				ResultId = resultId
			};

			_taskManager.Save(task1);
			_target.BeginExecuting();

			_taskManager.Save(task2);
			_target.BeginExecuting();

			_taskManager.Save(task3);
			_target.BeginExecuting();

			_taskManager.Save(task4);
			_target.BeginExecuting();


			Store.WaitForEntityToExist(resultId, 10);

			var result = Store.Load<ComputationResult<int>>(resultId);
			result.Result.Should().Be(4);
		}
	}
}
