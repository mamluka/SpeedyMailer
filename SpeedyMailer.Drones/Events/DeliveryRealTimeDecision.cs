using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Evens;

namespace SpeedyMailer.Drones.Events
{
	public class DeliveryRealTimeDecision : HappendOn<AggregatedMailBounced>
	{
		public override void Inspect(AggregatedMailBounced data)
		{
			
		}
	}
}