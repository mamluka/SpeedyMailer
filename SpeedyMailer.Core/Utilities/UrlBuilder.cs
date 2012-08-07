using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace SpeedyMailer.Core.Utilities
{
	public class UrlBuilder
	{
		private string _baseUrl;
		private readonly IList<string> _dataObjects = new List<string>();

		public UrlBuilder Base(string baseUrl)
		{
			_baseUrl = baseUrl;
			return this;
		}


		public UrlBuilder AddObject<T>(T dataObject)
		{
			_dataObjects.Add(ToBase64(dataObject));
			return this;
		}

		public static string ToBase64<T>(T whatToEncode)
		{
			var jsonObject = JsonConvert.SerializeObject(whatToEncode);
			var toEncodeAsBytes = Encoding.UTF8.GetBytes(jsonObject);
			var returnValue = Convert.ToBase64String(toEncodeAsBytes);

			return returnValue;
		}

		public string AppendAsSlashes()
		{
			return string.Format("{0}/{1}",_baseUrl,String.Join("/",_dataObjects));
		}
	}
}
