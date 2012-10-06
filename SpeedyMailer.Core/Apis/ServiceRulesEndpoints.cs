using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeedyMailer.Core.Apis
{
	public partial class ServiceEndpoints
	{
		public class Rules
		{
			public class Save:ApiCall
			{
				public Save() : base("/rules")
				{
					CallMethod = RestMethod.Post;
				}
			}
		}
	}
}
