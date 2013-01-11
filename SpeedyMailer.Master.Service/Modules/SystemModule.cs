using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using Raven.Client;
using SpeedyMailer.Core.Utilities.Extentions;

namespace SpeedyMailer.Master.Service.Modules
{
	public class SystemModule : NancyModule
	{
		public SystemModule(IDocumentStore documentStore)
			: base("/sys")
		{
			Get["/by-id"] = _ =>
				{
					using (var session = documentStore.OpenSession())
					{
						var id = (string)Request.Query["id"];
						if (!id.HasValue())
							return "No Id";

						var model = session.Load<object>(id);
						return Response.AsJson(model);
					}
				};

		}
	}
}
