using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using AttributeRouting.Web.Http;
using RestSharp;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Lists;

namespace SpeedyMailer.Master.Web.Api.Controllers
{
	public class ListsController : ApiController
	{
		private readonly SpeedyMailer.Core.Apis.Api _api;

		public ListsController(SpeedyMailer.Core.Apis.Api api)
		{
			_api = api;
		}

		[POST("/lists/upload"), HttpPost]
		public Task<string> Upload()
        {
            if (!Request.Content.IsMimeMultipartContent())
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

			var provider = new MultipartFormDataStreamProvider(HostingEnvironment.ApplicationPhysicalPath);

			var task = Request.Content.ReadAsMultipartAsync(provider);

			return task.ContinueWith(t =>
				                                 {
													 var file = t.Result.FileData.Select(x => x.LocalFileName).FirstOrDefault();

													 if (file == null)
														 throw new HttpRequestException("the file was not uploaded correctly");

													 _api
														 .AddFiles(new[] { file })
														 .Call<ServiceEndpoints.Lists.UploadContacts>(x =>
															                                        {
																                                        x.ListId = t.Result.FormData["listId"];
															                                        });

					                                 return "OK";
				                                 });

			
        }

		[POST("/lists/list"),HttpPost]
		public void CreateList(CreateListModel createListModel)
		{
			_api.Call<ServiceEndpoints.Lists.CreateList>(x => x.Name = createListModel.Name);
		}

		[GET("/lists/list"),HttpGet]
		public IList<ListDescriptor> GetLists()
		{
			return _api.Call<ServiceEndpoints.Lists.GetLists,List<ListDescriptor>>();
		}
	}

	public class CreateListModel
	{
		public string Name { get; set; }
	}
}
