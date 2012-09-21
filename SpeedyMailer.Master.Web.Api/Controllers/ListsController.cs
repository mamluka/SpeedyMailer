using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
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

		[POST("lists/start-upload"), HttpPost]
		public Task<string> Upload()
        {
            if (!Request.Content.IsMimeMultipartContent())
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

            var provider = new MultipartFormDataStreamProvider("c:/Users/cookie/Documents/SpeedyMailer/SpeedyMailer.Master.Web.Api/");

			var task = Request.Content.ReadAsMultipartAsync(provider);

			return task.ContinueWith(t =>
				                                 {
													 var file = t.Result.FileData.Select(x => x.LocalFileName).FirstOrDefault();

													 if (file == null)
														 throw new HttpRequestException("the file was not uploaded correctly");

													 _api
														 .AddFiles(new[] { file })
														 .Call<ServiceEndpoints.UploadContacts>(x =>
															                                        {
																                                        x.ListName = t.Result.FormData["listName"];
															                                        });

					                                 return "OK";
				                                 });

			
        }

		[POST("/lists/create"),HttpPost]
		public void CreateList(CreateListModel createListModel)
		{
			_api.Call<ServiceEndpoints.CreateList>(x => x.Name = createListModel.Name);
		}

		[GET("lists/get"),HttpGet]
		public IList<ListDescriptor> GetLists()
		{
			return _api.Call<ServiceEndpoints.GetLists,List<ListDescriptor>>();
		}
	}

	public class CreateListModel
	{
		public string Name { get; set; }
	}
}
