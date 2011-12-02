using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SpeedyMailer.ControlRoom.Website.Core.Builders;
using SpeedyMailer.ControlRoom.Website.Core.Models;
using SpeedyMailer.ControlRoom.Website.Core.ViewModels;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.Core.Lists;

namespace SpeedyMailer.ControlRoom.Website.Controllers
{
    public class EmailsController : Controller
    {
        private readonly IEmailCSVParser emailCSVParser;
        private readonly IViewModelBuilderWithBuildParameters<UploadListViewModel, IEmailCSVParser> emailUploadViewModelBuilder;
        private readonly IViewModelBuilder<UploadListViewModel> uploadListViewModelBuilder;

        public EmailsController(IEmailCSVParser emailCSVParser, IViewModelBuilderWithBuildParameters<UploadListViewModel, IEmailCSVParser> emailUploadViewModelBuilder, IViewModelBuilder<UploadListViewModel> uploadListViewModelBuilder)
        {
            this.emailCSVParser = emailCSVParser;
            this.emailUploadViewModelBuilder = emailUploadViewModelBuilder;
            this.uploadListViewModelBuilder = uploadListViewModelBuilder;
        }

        //
        // GET: /List/

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Upload()
        {
            var viewModel = uploadListViewModelBuilder.Build();
            return View(new UploadListViewModel());
        }
        [HttpPost]
        public ActionResult Upload(UploadListModel model)
        {
            emailCSVParser.ParseAndStore();
            var viewModel = emailUploadViewModelBuilder.Build(emailCSVParser);
            return View(viewModel);
        }

    }
}
