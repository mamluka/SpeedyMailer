using System;
using System.Web.UI.WebControls.WebParts;
using Newtonsoft.Json;
using RestSharp;

namespace SpeedyMailer.Core.Apis
{
	[JsonObject(MemberSerialization.OptOut)]
	public class ApiCall
	{
		protected ApiCall(string endpoint)
		{
			Endpoint = endpoint;
		}
		[JsonIgnoreAttribute]
		public string Endpoint { get; set; }

		[JsonIgnoreAttribute]
		public RestMethod CallMethod { get; set; }
	}

	public enum RestMethod
	{
		Get,
		Post,
		Put,
		Delete
	}
}