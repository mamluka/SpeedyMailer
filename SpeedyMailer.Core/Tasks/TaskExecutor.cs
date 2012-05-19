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

		public TaskExecutor(IDocumentStore documentStore, IKernel kernel)
		{
			_kernel = kernel;
			_documentStore = documentStore;
		}

		public void Start()
		{
			using (var session = _documentStore.OpenSession())
			{
				var tasks = session.Query<PersistentTask>()
					.Where(task => !task.Executed)
					.Customize(x=> x.WaitForNonStaleResults())
					.OrderBy(x => x.CreateDate).Take(3)
					.ToList();

				if (!tasks.Any()) return;

				foreach (var task in tasks)
				{
					MarkAsExecuted(session, task);

					try
					{
						var executorType = Type.GetType(task.GetType().FullName + "Executor");
						var executor = _kernel.Get(executorType);

						var executeMethod = executor.GetType().GetMethod("Execute");
						executeMethod.Invoke(executor, BindingFlags.Default, null, new object[] {task},null);
					}
					catch (Exception)
					{
						task.Executed = false;
						session.Store(task);
						session.SaveChanges();
					}
				}

				Start();
			}
		}

		private static void MarkAsExecuted(IDocumentSession session, PersistentTask task)
		{
			task.Executed = true;
			session.Store(task);
			session.SaveChanges();
		}
	}
}