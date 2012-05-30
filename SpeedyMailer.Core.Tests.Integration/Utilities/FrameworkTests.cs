using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Raven.Client;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Core.Tasks.Testing;
using SpeedyMailer.Core.Utilities;
using SpeedyMailer.Tests.Core;
using Rhino.Mocks;
using FluentAssertions;
using SpeedyMailer.Tests.Core.Integration.Base;
using SpeedyMailer.Tests.Core.Unit.Base;

namespace SpeedyMailer.Core.IntegrationTests.Utilities
{
	[TestFixture]
	public class FrameworkTests : IntegrationTestBase
	{
		private Framework _target;

		public override void ExtraSetup()
		{
			_target = MasterResolve<Framework>();
		}

		[Test]
		public void ExecuteCommand_WhenGivenACommand_ShouldExecuteIt()
		{
			var result = _target.ExecuteCommand(new AdditionCommand
			                                    	{
			                                    		FirstNumber = 2,
			                                    		SecondNumber = 2
			                                    	});

			result.Should().Be(4);
		}

		[Test]
		public void ExecuteCommand_WhenGivenAVoidCommand_ShouldExecuteIt()
		{
			_target.ExecuteCommand(new StoreInDatabaseCommand(DocumentStore));

			var result = Load<ComputationResult<string>>("result/1");

			result.Result.Should().Be("saved");
		}

		[Test]
		public void ExecuteTask_WhenGivenATask_ShouldExecuteAsyncAndNotReturnAValue()
		{
			var task = new AdditionTask
			           	{
			           		FirstNubmer = 2,
			           		SecondNumber = 2,
							ResultId = "result/1"
			           	};
			_target.SaveAndExecuteTask(task);

			var result = Load<ComputationResult<int>>("result/1");

			result.Should().BeNull();
		}

		[Test]
		public void ExecuteTask_WhenGivenATask_ShouldExecuteAsync()
		{
			const string resultId = "result/1";
			var task = new AdditionTask
			{
				FirstNubmer = 2,
				SecondNumber = 2,
				ResultId = resultId
			};
			_target.SaveAndExecuteTask(task);

			WaitForEntityToExist(resultId,10);
			var result = Load<ComputationResult<int>>(resultId);

			result.Result.Should().Be(4);
		}
	}

	public class StoreInDatabaseCommand : Command
	{
		private readonly IDocumentStore _documentStore;

		public StoreInDatabaseCommand(IDocumentStore documentStore)
		{
			_documentStore = documentStore;
		}

		public override void Execute()
		{
			using (var session = _documentStore.OpenSession())
			{
				session.Store(new ComputationResult<string>
				              	{
				              		Id = "result/1",
									Result = "saved"
				              	});
				session.SaveChanges();
			}
		}
	}

	public class AdditionCommand:Command<int>
	{
		public int FirstNumber { get; set; }
		public int SecondNumber { get; set; }

		public override int Execute()
		{
			return FirstNumber + SecondNumber;
		}
	}
}
