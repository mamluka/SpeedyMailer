using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SpeedyMailer.ControlRoom.Website.Core.ViewModels.Builders;
using SpeedyMailer.ControlRoom.Website.Core.ViewModels.ViewModels;
using SpeedyMailer.Core.Emails;

namespace SpeedyMailer.ControlRoom.Website.Controllers
{
    public class EmailsController : Controller
    {
        private readonly IEmailCSVParser emailCSVParser;
        private readonly IViewModelBuilderWithBuildParameters<EmailUploadListViewModel, IEmailCSVParser> emailUploadViewModelBuilder;

        public EmailsController(IEmailCSVParser emailCSVParser, IViewModelBuilderWithBuildParameters<EmailUploadListViewModel,IEmailCSVParser> emailUploadViewModelBuilder)
        {
            this.emailCSVParser = emailCSVParser;
            this.emailUploadViewModelBuilder = emailUploadViewModelBuilder;
        }

        //
        // GET: /List/

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Upload()
        {
            return View(new EmptyViewModel());
        }
        [HttpPost]
        public ActionResult UploadList()
        {
            emailCSVParser.ParseAndStore();
            var viewModel = emailUploadViewModelBuilder.Build(emailCSVParser);
            return View(viewModel);
        }

    }
}
