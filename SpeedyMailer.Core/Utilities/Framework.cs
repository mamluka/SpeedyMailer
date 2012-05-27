using System;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Tasks;

namespace SpeedyMailer.Core.Utilities
{
	public class Framework
	{
		private readonly ITaskManager _taskManager;
		private readonly ITaskExecutor _taskExecutor;

		public Framework(ITaskManager taskManager, ITaskExecutor taskExecutor)
		{
			_taskExecutor = taskExecutor;
			_taskManager = taskManager;
		}

		public void ExecuteCommand<T>(T command) where T : Command
		{
			command.Execute();
		}

		public void ExecuteCommand<T>(T command, Action<T> action) where T : Command
		{
			action.Invoke(command);
			command.Execute();
		}

		public TResult ExecuteCommand<T, TResult>(T command, Action<T> action) where T : Command<TResult>
		{
			action.Invoke(command);
			return command.Execute();
		}

		public void SaveAndExecuteTask(PersistentTask task)
		{
			_taskManager.Save(task);
			_taskExecutor.Start();
		}
	}
}