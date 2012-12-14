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
        public void SetDelivery(DeliverabilityClassificationRulesModel model)
        {
            _api.Call<ServiceEndpoints.Heuristics.SetDeliveryRules>(x => x.DeliverabilityClassificationRules = ToHeuristicsRules(model));
        }

        private DeliverabilityClassificationRules ToHeuristicsRules(DeliverabilityClassificationRulesModel model)
        {
            return new DeliverabilityClassificationRules
                {
                    HardBounceRules = model.HardBounceRules,
                    BlockingRules = model.BlockingRules.Select(x =>
                        {
                            string condition = x.Condition;
                            int timeSpan = x.TimeSpan;
                            return new HeuristicRule { Condition = condition, TimeSpan = TimeSpan.FromHours(timeSpan) };
                        }).ToList()
                };
        }

        [GET("heuristics/delivery")]
        public IList<DeliverabilityClassificationRules> GetDelivery()
        {
            var result = _api.Call<ServiceEndpoints.Heuristics.GetDeliveryRules, DeliverabilityClassificationRules>();
            return new[] { result };
        }
    }

    public class DeliverabilityClassificationRulesModel
    {
        public List<string> HardBounceRules { get; set; }
        public List<dynamic> BlockingRules { get; set; }
    }
}
