using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Raven.Client;
using SpeedyMailer.Core.Tasks;

namespace SpeedyMailer.Tests.Core.Integration.Base
{
	public class IntegrationTasksHelpers
	{
		private readonly IntegrationStoreHelpers _integrationStoreHelpers;

		public IntegrationTasksHelpers(IntegrationStoreHelpers integrationStoreHelpers)
		{
			_integrationStoreHelpers = integrationStoreHelpers;
		}

		public void WaitForTaskToComplete(string taskId, int secondsToWait = 30)
		{
			Func<IDocumentSession, Stopwatch, bool> condition =
				(session, stopwatch) =>
				session.Load<PersistentTask>(taskId).Status == PersistentTaskStatus.Executed &&
				stopwatch.ElapsedMilliseconds < secondsToWait * 1000;

			_integrationStoreHelpers.WaitForStoreWithFunction(condition);
		}
	}
}
