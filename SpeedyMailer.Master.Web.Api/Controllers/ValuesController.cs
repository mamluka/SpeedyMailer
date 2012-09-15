using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AttributeRouting.Web.Http;

namespace SpeedyMailer.Master.Web.Api.Controllers
{
	public class ValuesController : ApiController
	{
		// GET api/values
        [GET("/api/get-value")]
		public IEnumerable<string> Getthis()
		{
			return new string[] { "value1", "value2" };
		}

		// GET api/values/5
		public string Getthis(int id)
		{
			return "value";
		}

		// POST api/values
		public void Post([FromBody]string value)
		{
		}

		// PUT api/values/5
		public void Put(int id, [FromBody]string value)
		{
		}

		// DELETE api/values/5
		public void Delete(int id)
		{
		}
	}
}