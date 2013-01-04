using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Evens;

namespace SpeedyMailer.Drones.Events
{
	public class StopSendingWhenTheContentIsBlocked:IHappendOn<AggregatedMailBounced>
	{
		public void Inspect(AggregatedMailBounced data)
		{
			
		}
	}
}