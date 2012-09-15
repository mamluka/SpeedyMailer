using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AttributeRouting.Web.Http;

namespace SpeedyMailer.Master.Web.Api.Controllers
{
    public class UploadContactListController : ApiController
    {
        [POST("/lists/upload"), HttpPost]
        public Task<IEnumerable<string>> Upload()
        {
            if (!Request.Content.IsMimeMultipartContent())
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

            // Create the stream provider, and tell it sort files in my c:\temp\uploads folder
            var provider = new MultipartFormDataStreamProvider("c:/Users/cookie/Documents/SpeedyMailer/SpeedyMailer.Master.Web.Api/");

            // Read using the stream
            return Request.Content.ReadAsMultipartAsync(provider).ContinueWith(t =>
                                                                                            {
                                                                                                if (t.IsFaulted || t.IsCanceled)
                                                                                                    throw new HttpResponseException(HttpStatusCode.InternalServerError);

                                                                                                return
                                                                                                    provider.FileData.
                                                                                                        Select(
                                                                                                            x =>
                                                                                                            x.
                                                                                                                LocalFileName);
                                                                                            });

        }
    }
}
