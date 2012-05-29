using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Quartz;
using SpeedyMailer.Drone.Tasks;
using SpeedyMailer.Tests.Core;
using Rhino.Mocks;
using FluentAssertions;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Drone.Tests.Integration.Tasks
{
	[TestFixture]
	public class ScheduledTaskManagerTests : IntegrationTestBase
	{
		private IScheduledTaskManager _target;

		public override void ExtraSetup()
		{
			_target = DroneResolve<IScheduledTaskManager>();
		}

		[Test]
		public void Start_WhenGivenAScheduledTask_ShouldScheduleAndStartTheTask()
		{
			var testFile = Guid.NewGuid() + ".txt";
			var task = new TestScheduledTask(x =>
												{
													x.TestFile = testFile;
												});

			_target.Start(task);

			LetSchedulerThreadSpinAlive();
			DoesFileExist(testFile).Should().BeTrue();

			ClearnUpFile(testFile);
		}

		private void ClearnUpFile(string testFile)
		{
			File.Delete(testFile);
		}

		private static void LetSchedulerThreadSpinAlive()
		{
			Thread.Sleep(1000);
		}

		private static bool DoesFileExist(string testFile)
		{
			return File.Exists(testFile);
		}
	}

	public class TestScheduledTask : ScheduledTask
	{
		private readonly Data _data;

		public TestScheduledTask(Action<Data> action)
		{
			_data = new Data();
			action.Invoke(_data);
		}

		public override string Name
		{
			get { return "TestScheduledTask"; }
		}

		public override IJobDetail GetJob()
		{
			return SimpleJob<Job>(_data);
		}

		public override ITrigger GetTrigger()
		{
			return TriggerWithTimeCondition(x =>
												{
													x.WithIntervalInSeconds(5);
													x.WithRepeatCount(1);
												});
		}

		public override ScheduledTaskData GetData()
		{
			return _data;
		}

		public class Data : ScheduledTaskData
		{
			public string TestFile { get; set; }
		}

		public class Job : JobBase<Data>,IJob
		{
			public void Execute(IJobExecutionContext context)
			{
				var data = GetData(context);
				using (var file = new StreamWriter(data.TestFile, true))
				{
					file.WriteLine(context.JobRunTime);
				}
			}
		}
	}
}
