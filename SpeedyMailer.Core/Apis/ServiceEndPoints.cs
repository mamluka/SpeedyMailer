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
			public RegisterDrone():base("drones/register")
			{
				CallMethod = RestMethod.Post;
			}

			public string Identifier { get; set; }

			public string BaseUrl { get; set; }

			public string LastUpdate { get; set; }
		}

		public class FetchFragment : ApiCall
		{
			public FetchFragment() : base("fragments/fetch")
			{
				CallMethod = RestMethod.Get;
			}

			public class Response
			{
				public CreativeFragment CreativeFragment { get; set; }
			}
		}

		public class GetRemoteServiceSettings:ApiCall
		{
			public GetRemoteServiceSettings() : base("admin/settings/get")
			{
				CallMethod = RestMethod.Get;
			}

			public class Response
			{
				public string ServiceBaseUrl { get; set; }
			}
		}

	    public class UploadContacts:ApiCall
	    {
	        public string ListName { get; set; }

	        public UploadContacts() : base("/lists/upload-list")
	        {
                CallMethod = RestMethod.Put;
	        }
	    }

		public class GetLists:ApiCall
		{
			public GetLists() : base("/lists/get")
			{
				CallMethod = RestMethod.Get;
			}
		}

		public class CreateList:ApiCall
		{
			public string Name { get; set; }

			public CreateList() : base("/lists/create")
			{
				CallMethod = RestMethod.Post;
			}
		}
	}
}