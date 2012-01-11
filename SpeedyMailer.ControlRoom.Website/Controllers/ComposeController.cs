using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using SpeedyMailer.ControlRoom.Website.Core.Builders;
using SpeedyMailer.ControlRoom.Website.Core.Models;
using SpeedyMailer.ControlRoom.Website.Core.ViewModels;
using SpeedyMailer.Core.Emails;

namespace SpeedyMailer.ControlRoom.Website.Controllers
{
    public class ComposeController : Controller
    {
        private readonly IViewModelBuilder<ComposeViewModel> indexViewModelBuilder;
        private readonly IEmailPoolService emailPoolService;
        private readonly IMappingEngine mapper;
        //
        // GET: /Compose/

        public ComposeController(IViewModelBuilder<ComposeViewModel> indexViewModelBuilder, IEmailPoolService emailPoolService, IMappingEngine mapper)
        {
            this.indexViewModelBuilder = indexViewModelBuilder;
            this.emailPoolService = emailPoolService;
            this.mapper = mapper;
        }

        public ActionResult Index()
        {
            var viewModel = indexViewModelBuilder.Build();
            return View(viewModel);
        }
        [HttpPost]
        public ActionResult Index(ComposeModel composeModel)
        {
            var email = mapper.Map<ComposeModel, Email>(composeModel);
            emailPoolService.AddEmail(email);
            return null;
        }

    }
}
