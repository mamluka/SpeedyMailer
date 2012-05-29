using System;
using System.Diagnostics;
using System.Reflection;
using Ninject;
using Raven.Abstractions.Exceptions;
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
		private readonly ITaskExecutionSettings _taskExecutionSettings;

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
				session.Advanced.UseOptimisticConcurrency = true;
				var tasks = session.Query<PersistentTask>()
					.Where(task => task.Status == PersistentTaskStatus.Pending)
					.Customize(x => x.WaitForNonStaleResults())
					.OrderBy(x => x.CreateDate).Take(1)
					.ToList();

				if (!tasks.Any()) return;

				foreach (var task in tasks)
				{
					try
					{
						MarkAs(task, session, PersistentTaskStatus.Executing);
					}
					catch (ConcurrencyException)
					{
						continue;
					}

					try
					{
						var executorType = Type.GetType(GetExecutorQualifiedName(task));
						var executor = _kernel.Get(executorType);

						var executeMethod = executor.GetType().GetMethod("Execute");
						executeMethod.Invoke(executor, BindingFlags.Default, null, new object[] { task }, null);

						MarkAs(task, session, PersistentTaskStatus.Executed);
					}
					catch (Exception)
					{
						if (task.RetryCount >= _taskExecutionSettings.NumberOfRetries)
						{
							MarkAs(task, session, PersistentTaskStatus.Failed);
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

		private static string GetExecutorQualifiedName(PersistentTask task)
		{
			var qualifiedNameSplit = task.GetType().AssemblyQualifiedName.Split(',');
			qualifiedNameSplit[0] += "Executor";

			return String.Join(",", qualifiedNameSplit);
		}

		private void MarkAs(PersistentTask task, IDocumentSession session, PersistentTaskStatus persistentTaskStatus)
		{
			task.Status = persistentTaskStatus;
			session.Store(task);
			session.SaveChanges();
		}

		private void IncreaseRetryCount(IDocumentSession session, PersistentTask task)
		{
			task.RetryCount++;
			MarkAs(task, session, PersistentTaskStatus.Pending);
		}
	}
}