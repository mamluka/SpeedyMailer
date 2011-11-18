using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SpeedyMailer.ControlRoom.Website.ViewModels.Builders;
using SpeedyMailer.ControlRoom.Website.ViewModels.ViewModels;
using SpeedyMailer.Core.Emails;

namespace SpeedyMailer.ControlRoom.Website.Controllers
{
    public class EmailsController : Controller
    {
        private readonly IEmailCSVParser emailCSVParser;
        private readonly IViewModelBuilder<EmailUploadViewModel> emailUploadViewModelBuilder;

        public EmailsController(IEmailCSVParser emailCSVParser, IViewModelBuilder<EmailUploadViewModel> emailUploadViewModelBuilder)
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
            emailCSVParser.Parse();
            emailUploadViewModelBuilder.Build();
            return View();
        }

    }
}
