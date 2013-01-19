using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AttributeRouting.Web.Http;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Rules;

namespace SpeedyMailer.Master.Web.Api.Controllers
{
	public class RulesController : ApiController
	{
		private SpeedyMailer.Core.Apis.Api _api;

		public RulesController(SpeedyMailer.Core.Apis.Api api)
		{
			_api = api;
		}

		[POST("/rule")]
		public void SaveRule(RuleModel ruleModel)
		{
			_api.Call<ServiceEndpoints.Rules.AddIntervalRules>(x =>
																   {
																	   x.IntervalRules = new List<IntervalRule>
						                                                                     {
							                                                                     new IntervalRule
								                                                                     {
																										 Conditons = ruleModel.Conditions,
																										 Group = ruleModel.Group,
																										 Interval = ruleModel.Interval
								                                                                     }
						                                                                     };
																   });
		}

		[GET("/rule")]
		public IList<IntervalRule> GetRules()
		{
			return _api.Call<ServiceEndpoints.Rules.GetIntervalRules, List<IntervalRule>>();
		} 
	}

	public class RuleModel
	{
		public int Interval { get; set; }
		public string Group { get; set; }
		public List<string> Conditions { get; set; }
		public RuleType Type { get; set; }
	}

	public enum RuleType
	{
		Interval
	}
}
