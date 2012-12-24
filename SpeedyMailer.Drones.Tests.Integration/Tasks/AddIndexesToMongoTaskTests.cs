using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Quartz;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Drones.Storage;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Drones.Tests.Integration.Tasks
{
	public class AddIndexesToMongoTaskTests : IntegrationTestBase
	{
		public AddIndexesToMongoTaskTests()
			: base(x => x.UseMongo = true)
		{ }

		[Test]
		public void Execute_WhenExecuted_ShouldWriteTheCreativePackagesIndexes()
		{
			var tasl = new AddIndexesToMongoTask();
		}
	}

	public class AddIndexesToMongoTask : ScheduledTask
	{
		public override IJobDetail ConfigureJob()
		{
			return SimpleJob<Job>();
		}

		public override ITrigger ConfigureTrigger()
		{
			return TriggerWithTimeCondition(x => x.WithRepeatCount(1));
		}

		public class Job : IJob
		{
			private OmniRecordManager _omniRecordManager;

			public Job(OmniRecordManager omniRecordManager)
			{
				_omniRecordManager = omniRecordManager;
			}

			public void Execute(IJobExecutionContext context)
			{
				_omniRecordManager.EnsureIndex<CreativePackage>(x => x.Group, x => x.Processed);
			}
		}
	}
}
