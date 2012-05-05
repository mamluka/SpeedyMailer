using System;
using Raven.Client;
using RestSharp;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Domain.Emails;
using SpeedyMailer.Core.Protocol;
using SpeedyMailer.Core.Settings;

namespace SpeedyMailer.Master.Web.Core.Commands
{
	public class SendCreativeCommand:Command
	{
		private readonly IDocumentStore _documentStore;
		private readonly IRestClient _restClient;
		private readonly ICreativeApisSettings _creativeApisSettings;
	    private readonly IBaseApiSettings _baseApiSettings;
	    public string CreativeId { get; set; }
	    public string UnsubscribedTemplateId { get; set; }

	    public SendCreativeCommand(IDocumentStore documentStore,IRestClient restClient,IBaseApiSettings baseApiSettings,ICreativeApisSettings creativeApisSettings)
		{
		    _baseApiSettings = baseApiSettings;
		    _creativeApisSettings = creativeApisSettings;
			_restClient = restClient;
			_documentStore = documentStore;
		}

		public override void Execute()
		{
			using (var session = _documentStore.OpenSession())
			{
				var restRequest = new RestRequest(_creativeApisSettings.AddCreative);
			    var request = new CreativeApi.Add.Request
			                      {
                                      CreativeId = CreativeId,
                                      UnsubscribedTemplateId = UnsubscribedTemplateId
			                      };

				restRequest.AddBody(creative);
			    _restClient.BaseUrl = _baseApiSettings.ServiceBaseUrl;
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
}