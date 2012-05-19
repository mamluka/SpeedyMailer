using System;
using System.Reflection;
using Ninject;
using Raven.Client;
using Raven.Client.Linq;
using Raven.Client.Extensions;
using System.Linq;

namespace SpeedyMailer.Core.Tasks
{
	public interface ITaskExecutor
	{
		void Start();
	}

	public class TaskExecutor : ITaskExecutor
	{
		private readonly IDocumentStore _documentStore;
		private readonly IKernel _kernel;
		private ITaskExecutionSettings _taskExecutionSettings;

		public TaskExecutor(IKernel kernel, IDocumentStore documentStore, ITaskExecutionSettings taskExecutionSettings)
		{
			_taskExecutionSettings = taskExecutionSettings;
			_kernel = kernel;
			_documentStore = documentStore;
		}

		public void Start()
		{
			using (var session = _documentStore.OpenSession())
			{
				var tasks = session.Query<PersistentTask>()
					.Where(task => task.Status == PersistentTaskStatus.Pending)
					.Customize(x=> x.WaitForNonStaleResults())
					.OrderBy(x => x.CreateDate).Take(3)
					.ToList();

				if (!tasks.Any()) return;

				foreach (var task in tasks)
				{
					MarkAs(session, task, PersistentTaskStatus.Executing);

					try
					{
						var executorType = Type.GetType(task.GetType().FullName + "Executor");
						var executor = _kernel.Get(executorType);

						var executeMethod = executor.GetType().GetMethod("Execute");
						executeMethod.Invoke(executor, BindingFlags.Default, null, new object[] {task},null);

						MarkAs(session, task, PersistentTaskStatus.Executed);
					}
					catch (Exception)
					{
						if (task.RetryCount >= _taskExecutionSettings.NumberOfRetries)
						{
							MarkAs(session, task, PersistentTaskStatus.Failed);
						}
						else
						{
							IncreaseRetryCount(session, task);
						}
					}
				}

				Start();
			}
		}

		private void MarkAs(IDocumentSession session, PersistentTask task, PersistentTaskStatus persistentTaskStatus)
		{
			task.Status = persistentTaskStatus;
			session.Store(task);
			session.SaveChanges();
		}

		private void IncreaseRetryCount(IDocumentSession session, PersistentTask task)
		{
			task.RetryCount++;
			MarkAs(session, task, PersistentTaskStatus.Pending);
		}
	}
}