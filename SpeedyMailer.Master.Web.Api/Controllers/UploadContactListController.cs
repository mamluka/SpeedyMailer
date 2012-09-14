using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AttributeRouting.Web.Http;
using RestSharp;

namespace SpeedyMailer.Master.Web.Api.Controllers
{
	public class UploadContactListController : ApiController
	{
		[POST("/lists/upload"), HttpPost]
		public IList<string> Upload()
        {
            if (!Request.Content.IsMimeMultipartContent())
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

            // Create the stream provider, and tell it sort files in my c:\temp\uploads folder
            var provider = new MultipartFormDataStreamProvider("c:/Users/cookie/Documents/SpeedyMailer/SpeedyMailer.Master.Web.Api/");

            // Read using the stream
			var task = Request.Content.ReadAsMultipartAsync(provider);
			task.Wait();

			return task.Result.FileData.Select(x => x.LocalFileName).ToList();
        }
	}
}
