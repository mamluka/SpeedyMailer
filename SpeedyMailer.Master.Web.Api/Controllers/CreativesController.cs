using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AttributeRouting.Web.Http;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Creative;

namespace SpeedyMailer.Master.Web.Api.Controllers
{
	public class CreativesController : ApiController
	{
		private SpeedyMailer.Core.Apis.Api _api;

		public CreativesController(SpeedyMailer.Core.Apis.Api api)
		{
			_api = api;
		}

		[POST("/creatives"), HttpPost]
		public void SaveCreative(CreativeModel creativeModel)
		{
			_api.Call<ServiceEndpoints.Creative.SaveCreative>(x =>
														 {
															 x.Body = creativeModel.Body;
															 x.DealUrl = creativeModel.DealUrl;
															 x.Subject = creativeModel.Subject;
															 x.ListId = creativeModel.ListId;
															 x.UnsubscribeTemplateId = creativeModel.TemplateId;
														 });
		}

		[GET("/creatives"), HttpGet]
		public IList<CreativeModel> GetAll()
		{
			return _api.Call<ServiceEndpoints.Creative.GetAll, List<Creative>>()
				.Select(ToCreativeModel).ToList();
		}

		private CreativeModel ToCreativeModel(Creative creative)
		{
			return new CreativeModel
					   {
						   Body = creative.Body,
						   DealUrl = creative.DealUrl,
						   ListId = creative.Lists.FirstOrDefault(),
						   Subject = creative.Subject,
						   TemplateId = creative.UnsubscribeTemplateId,
						   Id = creative.Id
					   };
		}
	}

	public class CreativeModel
	{
		public string Body { get; set; }
		public string DealUrl { get; set; }
		public string Subject { get; set; }
		public string ListId { get; set; }
		public string TemplateId { get; set; }
		public string Id { get; set; }
	}
}
