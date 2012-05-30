using System;
using Ninject;

namespace SpeedyMailer.Tests.Core.Integration.Base
{
	public class DroneActions : ActionsBase
	{
		public DroneActions(IKernel kernel)
			: base(kernel)
		{ }

		public override void EditSettings<T>(Action<T> expression)
		{

		}

		public void Initialize()
		{
			
		}

		public void Start()
		{

		}
	}
}