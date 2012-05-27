using System;
using System.Linq.Expressions;
using Ninject;
using Ploeh.AutoFixture;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Tasks;

namespace SpeedyMailer.Tests.Core.Integration.Base
{
	public abstract class ActionsBase
	{
		public readonly IKernel Kernel;
		private readonly ITaskManager _taskManager;
		private readonly ITaskExecutor _taskExecutor;

		public Fixture Fixture { get; private set; }

		protected ActionsBase(IKernel kernel, ITaskManager taskManager, ITaskExecutor taskExecutor)
		{
			_taskExecutor = taskExecutor;
			_taskManager = taskManager;
			Kernel = kernel;
			Fixture = new Fixture();
		}

		public void ExecuteCommand<T>() where T : Command
		{
			var command = Kernel.Get<T>();
			command.Execute();
		}

		public void ExecuteCommand<T>(Action<T> action) where T : Command
		{
			var command = Kernel.Get<T>();
			action.Invoke(command);
			command.Execute();
		}

		public TResult ExecuteCommand<T, TResult>(Action<T> action) where T : Command<TResult>
		{
			var command = Kernel.Get<T>();
			action.Invoke(command);
			return command.Execute();
		}

		public void SaveTask(PersistentTask task)
		{
			_taskManager.Save(task);
		}

		public void SaveAndExecuteTask(PersistentTask task)
		{
			_taskManager.Save(task);
			_taskExecutor.Start();
		}


		public abstract void EditSettings<T>(Action<T> expression);
	}
}