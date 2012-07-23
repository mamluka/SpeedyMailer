using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Domain.Creative;

namespace SpeedyMailer.Drones.Commands
{
	public class SendCreativePackageCommand:Command
	{
		public CreativePackage Package { get; set; }

		public override void Execute()
		{
			
		}
	}
}