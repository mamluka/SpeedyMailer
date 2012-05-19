using Raven.Client;

namespace SpeedyMailer.Core.Tasks.Testing
{
	public class AdditionTask : PersistentTask
	{
		public int FirstNubmer { get; set; }
		public int SecondNumber { get; set; }
		public string ResultId { get; set; }
	}

	public class AdditionTaskExecutor:PersistentTaskExecutor<AdditionTask>
	{
		private readonly IDocumentStore _documentStore;

		public AdditionTaskExecutor(IDocumentStore documentStore)
		{
			_documentStore = documentStore;
		}

		public override void Execute(AdditionTask task)
		{
			using (var session = _documentStore.OpenSession())
			{
				var result = new ComputationResult<int>
				             	{
									Id = task.ResultId,
				             		Result = task.FirstNubmer + task.SecondNumber
				             	};

				session.Store(result);
				session.SaveChanges();
			}
		}
	}
}