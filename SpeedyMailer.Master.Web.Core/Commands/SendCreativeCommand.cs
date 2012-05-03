using System;
using Raven.Client;
using RestSharp;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Domain.Emails;

namespace SpeedyMailer.Master.Web.Core.Commands
{
	public class SendCreativeCommand:Command
	{
		private readonly IDocumentStore _documentStore;
		private readonly IRestClient _restClient;
		private readonly ICreativeApisSettings _creativeApisSettings;
		public string CreativeId { get; set; }

		public SendCreativeCommand(IDocumentStore documentStore,IRestClient restClient,ICreativeApisSettings creativeApisSettings)
		{
			_creativeApisSettings = creativeApisSettings;
			_restClient = restClient;
			_documentStore = documentStore;
		}

		public override void Execute()
		{
			using (var session = _documentStore.OpenSession())
			{
				var creative = session.Load<Email>(CreativeId);

				var restRequest = new RestRequest(new Uri(_creativeApisSettings.AddCreative));
				restRequest.AddBody(creative);
				_restClient.Execute<FaultTolerantResponse>(restRequest);
			}
		}
        
	}

	public class FaultTolerantResponse
	{
		public bool ExceptionOccured;
		public string ExceptionMessage { get; set; }
		public string ExceptionName { get; set; }
	}

	public class FaultTolerantResponse<T>:FaultTolerantResponse
	{
		public T Model;
	}

	public interface ICreativeApisSettings
	{
		string AddCreative { get; set; }
	}
}