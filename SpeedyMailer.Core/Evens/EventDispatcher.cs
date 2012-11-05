using System.Linq;
using Ninject;

namespace SpeedyMailer.Core.Evens
{
	public class EventDispatcher
	{
		private readonly IKernel _kernel;

		public EventDispatcher(IKernel kernel)
		{
			_kernel = kernel;
		}

		public void ExecuteAll<TEventData>(TEventData eventData)
		{
			_kernel.GetAll<HappendOn<TEventData>>()
				.ToList()
				.ForEach(x=> x.Inspect(eventData));
		}
	}
}