using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using Nancy.Responses;
using SpeedyMailer.Core.Domain;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Utilities;
using SpeedyMailer.Drones.Storage;

namespace SpeedyMailer.Drones.Modules
{
	public class DealsModule : NancyModule
	{
		private readonly OmniRecordManager _omniRecordManager;

		public DealsModule(OmniRecordManager omniRecordManager)
			: base("/deals")
		{
			_omniRecordManager = omniRecordManager;
			Get["/{data}"] = call =>
								 {
									 string objectString = call.data;
									 var data = UrlBuilder.DecodeBase64<DealUrlData>(objectString);

									 var creativeToDealMap = _omniRecordManager.Load<CreativeToDealMap>(data.CreativeId);

									 return new RedirectResponse(creativeToDealMap.DealUrl, RedirectResponse.RedirectType.Permanent);
								 };
		}
	}
}
