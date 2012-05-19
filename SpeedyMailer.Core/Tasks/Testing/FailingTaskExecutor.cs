using System;

namespace SpeedyMailer.Core.Tasks.Testing
{
	public class FailingTaskExecutor:PersistentTaskExecutor<FailingTask>
	{
		public override void Execute(FailingTask task)
		{
			throw new Exception("failing...");
		}
	}
}