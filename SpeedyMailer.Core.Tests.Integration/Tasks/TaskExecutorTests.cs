using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Core.Tasks.Testing;
using SpeedyMailer.Tests.Core;
using Rhino.Mocks;
using FluentAssertions;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Core.IntegrationTests.Tasks
{
	[TestFixture]
	public class TaskExecutorTests : IntegrationTestBase
	{
		private ITaskExecutor _target;
		private ITaskManager _taskManager;

		public override void ExtraSetup()
		{
			_taskManager = MasterResolve<ITaskManager>();
			_target = MasterResolve<ITaskExecutor>();
		}

		[Test]
		public void Start_WhenThereIsATaskToBeExecuted_ShouldExecuteIt()
		{
			const string resultId = "addition/1";
			CreateTask(resultId, 2, 2);

			_target.Start();

			var result = Load<ComputationResult<int>>(resultId);
			result.Result.Should().Be(4);
		}

		[Test]
		public void Start_WhenThereAreMoreThenOneTask_ShouldExecuteThemAllUntilNoneLeft()
		{
			const string resultId1 = "addition/1";
			const string resultId2 = "addition/2";
			const string resultId3 = "addition/3";

			CreateTask(resultId1, 2, 2);
			CreateTask(resultId2, 3, 3);
			CreateTask(resultId3, 4, 4);

			_target.Start();

			var firstResult = Load<ComputationResult<int>>(resultId1);
			var secondResult = Load<ComputationResult<int>>(resultId2);
			var thirdResult = Load<ComputationResult<int>>(resultId3);

			firstResult.Result.Should().Be(4);
			secondResult.Result.Should().Be(6);
			thirdResult.Result.Should().Be(8);
		}

		[Test]
		public void Start_WhenTaskWasAlreadyExecuted_ShouldNotExecuteItAgain()
		{
			const string resultId = "addition/1";
			CreateTask(resultId, 2, 2);

			_target.Start();
			Delete<ComputationResult<int>>(resultId);

			_target.Start();

			var result = Load<ComputationResult<int>>(resultId);
			result.Should().BeNull();
		}

		[Test]
		public void Start_WhenGivenMoreTasksThenWeLoadEachStartIteration_ShouldLoadTheExtraTasksUsingRecursion()
		{
			const string resultId1 = "addition/1";
			const string resultId2 = "addition/2";
			const string resultId3 = "addition/3";
			const string resultId4 = "addition/4";

			CreateTask(resultId1, 2, 2);
			CreateTask(resultId2, 3, 3);
			CreateTask(resultId3, 4, 4);
			CreateTask(resultId4, 5, 5);

			_target.Start();

			var fourhResult = Load<ComputationResult<int>>(resultId4);

			fourhResult.Result.Should().Be(10);
		}

		[Test]
		public void Start_WhenTaskThrowsAnException_ShouldRetryUntilConsideredFailed()
		{

		}
		private void CreateTask(string resultId, int first, int second)
		{
			var task = new AdditionTask
						{
							ResultId = resultId,
							FirstNubmer = first,
							SecondNumber = second
						};

			_taskManager.Save(task);
		}
	}
}
