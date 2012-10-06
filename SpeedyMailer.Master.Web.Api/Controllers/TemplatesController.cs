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
	public class TemplatesController : ApiController
	{
		private readonly SpeedyMailer.Core.Apis.Api _api;

		public TemplatesController(SpeedyMailer.Core.Apis.Api api)
		{
			_api = api;
		}

		[GET("/templates/{templateType}"), HttpGet]
		public IList<Template> GetTemplates(string templateType)
		{
			return _api.Call<ServiceEndpoints.Templates.GetTemplates, List<Template>>(x => x.Type = (TemplateType)Enum.Parse(typeof(TemplateType), templateType, true));
		}

		[POST("/templates/{templateType}"), HttpPost]
		public void CreateTemplates(string templateType, CreateTempalteModel createTempalteModel)
		{
			_api.Call<ServiceEndpoints.Templates.CreateUnsubscribeTemplate>(x => x.Body = createTempalteModel.Body);
		}

		[GET("/templates/types/list"), HttpGet]
		public dynamic GetTemplatesTypes()
		{
			return Enum.GetNames(typeof(TemplateType)).Select(x => new
																	   {
																		   name = x
																	   }).ToList();
		}
	}

	public class CreateTempalteModel
	{
		public string Body { get; set; }
	}
}
