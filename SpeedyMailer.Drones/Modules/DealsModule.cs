using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using Nancy.Responses;
using SpeedyMailer.Core.Domain;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Domain.Emails;
using SpeedyMailer.Core.Utilities;
using SpeedyMailer.Drones.Storage;

namespace SpeedyMailer.Drones.Modules
{
	public class DealsModule : NancyModule
	{
		public DealsModule(OmniRecordManager omniRecordManager)
			: base("/deals")
		{
			Get["/{data}"] = call =>
								 {
									 string objectString = call.data;
									 var data = UrlBuilder.DecodeBase64<DealUrlData>(objectString);



									 var creativeToDealMap = omniRecordManager.Load<CreativeToDealMap>(data.CreativeId);

									 omniRecordManager.UpdateOrInsert(new ClickAction
																		   {
																			   ContactId = data.ContactId,
																			   CreativeId = data.CreativeId,
																			   Date = DateTime.UtcNow
																		   });

									 return new RedirectResponse(creativeToDealMap.DealUrl, RedirectResponse.RedirectType.Permanent);
								 };
		}
	}
}
