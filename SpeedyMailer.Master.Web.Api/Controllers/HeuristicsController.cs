using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AttributeRouting.Web.Http;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Mail;

namespace SpeedyMailer.Master.Web.Api.Controllers
{
	public class HeuristicsController : ApiController
	{
		private readonly SpeedyMailer.Core.Apis.Api _api;

		public HeuristicsController(SpeedyMailer.Core.Apis.Api api)
		{
			_api = api;
		}

		[POST("heuristics/delivery")]
		public void SetDelivery(DeliverabilityClassificationRules model)
		{
			_api.Call<ServiceEndpoints.Heuristics.SetDeliveryRules>(x => x.DeliverabilityClassificationRules = model);
		}

		[GET("heuristics/delivery")]
		public IList<DeliverabilityClassificationRules> GetDelivery()
		{
			var result = _api.Call<ServiceEndpoints.Heuristics.GetDeliveryRules, DeliverabilityClassificationRules>();
			return new[] {result};
		}
	}
}
