using System;
using Ninject;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Tasks;

namespace SpeedyMailer.Tests.Core.Integration.Base
{
	public abstract class ActionsBase
	{
		private readonly IKernel _kernel;
		private readonly ITaskManager _taskManager;
		private readonly ITaskExecutor _taskExecutor;
		private readonly IScheduledTaskManager _scheduledTaskManager;

		protected ActionsBase(IKernel kernel, ITaskManager taskManager, ITaskExecutor taskExecutor, IScheduledTaskManager scheduledTaskManager)
		{
			_scheduledTaskManager = scheduledTaskManager;
			_taskExecutor = taskExecutor;
			_taskManager = taskManager;
			_kernel = kernel;
		}

		public void ExecuteCommand<T>() where T : Command
		{
			var command = _kernel.Get<T>();
			command.Execute();
		}

		public void ExecuteCommand<T>(Action<T> action) where T : Command
		{
			var command = _kernel.Get<T>();
			action.Invoke(command);
			command.Execute();
		}

		public TResult ExecuteCommand<T, TResult>(Action<T> action) where T : Command<TResult>
		{
			var command = _kernel.Get<T>();
			action.Invoke(command);
			return command.Execute();
		}

		public abstract void EditSettings<T>(Action<T> expression) where T : class,new();

		public void SaveTask(PersistentTask task)
		{
			_taskManager.Save(task);
		}

		public string ExecuteTask(PersistentTask task)
		{
			var taskId = _taskManager.Save(task);
			_taskExecutor.Start();
			return taskId;
		}

		public void StartScheduledTask(ScheduledTask scheduledTask)
		{
			_scheduledTaskManager.AddAndStart(scheduledTask);
		}
	}
}