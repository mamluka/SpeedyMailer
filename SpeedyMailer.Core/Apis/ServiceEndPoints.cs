using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Settings;

namespace SpeedyMailer.Core.Apis
{
	public class ServiceEndpoints
	{
		public class RegisterDrone : ApiCall
		{
			public RegisterDrone()
				: base("/drones")
			{
				CallMethod = RestMethod.Post;
			}

			public string Identifier { get; set; }

			public string BaseUrl { get; set; }

			public string LastUpdate { get; set; }
		}

		public class FetchFragment : ApiCall
		{
			public FetchFragment()
				: base("/creative/fragments")
			{
				CallMethod = RestMethod.Get;
			}

			public class Response
			{
				public CreativeFragment CreativeFragment { get; set; }
			}
		}

		public class GetRemoteServiceSettings : ApiCall
		{
			public GetRemoteServiceSettings()
				: base("/admin/settings")
			{
				CallMethod = RestMethod.Get;
			}

			public class Response
			{
				public string ServiceBaseUrl { get; set; }
			}
		}

		public class UploadContacts : ApiCall
		{
			public string ListName { get; set; }

			public UploadContacts()
				: base("/lists/upload-list")
			{
				CallMethod = RestMethod.Put;
			}
		}

		public class GetLists : ApiCall
		{
			public GetLists()
				: base("/lists")
			{
				CallMethod = RestMethod.Get;
			}
		}

		public class CreateList : ApiCall
		{
			public string Name { get; set; }

			public CreateList()
				: base("/lists")
			{
				CallMethod = RestMethod.Post;
			}
		}

		public class SaveCreative : ApiCall
		{
			public string Body { get; set; }
			public string ListId { get; set; }
			public string Subject { get; set; }
			public string DealUrl { get; set; }
			public string UnsubscribeTemplateId { get; set; }

			public SaveCreative()
				: base("/creative/save")
			{
				CallMethod = RestMethod.Post;
			}
		}

		public class CreateUnsubscribeTemplate : ApiCall
		{
			public string Body { get; set; }

			public CreateUnsubscribeTemplate()
				: base("/templates/unsubscribe")
			{
				CallMethod = RestMethod.Post;
			}
		}

		public class GetTemplates : ApiCall
		{
			public TemplateType Type { get; set; }

			public GetTemplates()
				: base("/templates/{type}")
			{
				CallMethod = RestMethod.Get;
			}
		}

        public class Send : ApiCall
        {
            public Send()
                : base("/creative/send")
            {
                CallMethod = RestMethod.Post;
            }

            public string CreativeId { get; set; }

            public class Response
            {

            }
        }
	}
}