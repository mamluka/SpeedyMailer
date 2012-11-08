using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Evens;

namespace SpeedyMailer.Drones.Events
{
	public class DeliveryRealTimeDecision : HappendOn<MailEvent>
	{
		public override void Inspect(MailEvent data)
		{

		}
	}
}