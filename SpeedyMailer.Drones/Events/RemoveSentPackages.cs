using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Evens;

namespace SpeedyMailer.Drones.Events
{
	public class RemoveSentPackages:IHappendOn<AggregatedMailSent>
	{
		public void Inspect(AggregatedMailSent data)
		{
			
		}
	}
}