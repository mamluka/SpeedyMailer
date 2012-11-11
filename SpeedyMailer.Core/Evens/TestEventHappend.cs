using Raven.Client;
using SpeedyMailer.Core.Tasks;

namespace SpeedyMailer.Core.Evens
{
	public class TestEventHappend:IHappendOn<TestEventData>
	{
		private readonly IDocumentStore _documentStore;

		public TestEventHappend(IDocumentStore documentStore)
		{
			_documentStore = documentStore;
		}

		public void Inspect(TestEventData data)
		{
			using (var session = _documentStore.OpenSession())
			{
				var result = new ComputationResult<int>
					             {
						             Id = data.ResultId,
						             Result = 2
					             };

				session.Store(result);
				session.SaveChanges();
			}
		}
	}
	
	public class AnotherTestEventHappend:IHappendOn<TestEventData>
	{
		private readonly IDocumentStore _documentStore;

		public AnotherTestEventHappend(IDocumentStore documentStore)
		{
			_documentStore = documentStore;
		}

		public void Inspect(TestEventData data)
		{
			using (var session = _documentStore.OpenSession())
			{
				var result = new ComputationResult<int>
					             {
						             Id = data.SecondResultId,
						             Result = 2
					             };

				session.Store(result);
				session.SaveChanges();
			}
		}
	}
}