using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using SpeedyMailer.Core.Domain.Creative;

namespace SpeedyMailer.Core.Utilities
{
	public class UrlBuilder
	{
		private string _baseUrl;
		private IList<string> _dataObjects = new List<string>();

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

		public UrlBuilder AddString(string dataString)
		{
			_dataObjects.Add(ToBase64(dataString));
			return this;
		}

		public string AppendAsSlashes()
		{
			var finelUrl = string.Format("{0}/{1}", _baseUrl, String.Join("/", _dataObjects));
			_dataObjects = new List<string>();

			return finelUrl;
		}

		public static string ToBase64<T>(T whatToEncode)
		{
			var jsonObject = JsonConvert.SerializeObject(whatToEncode);
			var toEncodeAsBytes = Encoding.UTF8.GetBytes(jsonObject);
			var returnValue = Convert.ToBase64String(toEncodeAsBytes);

			return returnValue;
		}

		public static string ToBase64(string whatToEncode)
		{
			var toEncodeAsBytes = Encoding.UTF8.GetBytes(whatToEncode);
			var returnValue = Convert.ToBase64String(toEncodeAsBytes);

			return returnValue;
		}

		public static T DecodeBase64<T>(string whatToDecode)
		{
			var encodedDataAsBytes = Convert.FromBase64String(whatToDecode);
			var stringObject = Encoding.UTF8.GetString(encodedDataAsBytes);
			return JsonConvert.DeserializeObject<T>(stringObject);
		}

		public static string DecodeBase64(string whatToDecode)
		{
			var encodedDataAsBytes = Convert.FromBase64String(whatToDecode);
			return Encoding.UTF8.GetString(encodedDataAsBytes);
		}
	}
}
