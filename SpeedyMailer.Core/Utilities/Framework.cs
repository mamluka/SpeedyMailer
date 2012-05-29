using System;
using Raven.Client;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Tasks;

namespace SpeedyMailer.Core.Utilities
{
	public class Framework
	{
		private readonly ITaskManager _taskManager;
		private readonly ITaskExecutor _taskExecutor;
		private readonly IDocumentStore _documentStore;

		public Framework(IDocumentStore documentStore, ITaskManager taskManager, ITaskExecutor taskExecutor)
		{
			_documentStore = documentStore;
			_taskExecutor = taskExecutor;
			_taskManager = taskManager;
		}

		public void ExecuteCommand(Command command)
		{
			command.Execute();
		}

		public TResult ExecuteCommand<TResult>(Command<TResult> command)
		{
			return command.Execute();
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

		public void Store(dynamic entity)
		{
			using (var session = _documentStore.OpenSession())
			{
				session.Store(entity);
				session.SaveChanges();
			}
		}
	}
}