using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeedyMailer.Core.Apis
{
	public partial class ServiceEndpoints
	{
		public class Lists
		{
			public class UploadContacts : ApiCall
			{
				public string ListId { get; set; }

				public UploadContacts()
					: base("/lists/upload-list")
				{
					CallMethod = RestMethod.Put;
				}
			}

			public class GetLists : ApiCall
			{
				public GetLists()
					: base("/lists")
				{
					CallMethod = RestMethod.Get;
				}
			}

			public class CreateList : ApiCall
			{
				public string Name { get; set; }

				public CreateList()
					: base("/lists")
				{
					CallMethod = RestMethod.Post;
				}
			}
		}
	}
}
