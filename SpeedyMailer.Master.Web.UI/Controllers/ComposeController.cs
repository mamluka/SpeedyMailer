using System.Web.Mvc;
using AutoMapper;
using SpeedyMailer.ControlRoom.Website.Core.Builders;
using SpeedyMailer.ControlRoom.Website.Core.Models;
using SpeedyMailer.ControlRoom.Website.Core.ViewModels;
using SpeedyMailer.Core.Emails;

namespace SpeedyMailer.Master.Web.UI.Controllers
{
    public class ComposeController : Controller
    {
        private readonly IViewModelBuilder<ComposeViewModel> indexViewModelBuilder;
        private readonly IEmailRepository emailRepository;
        private readonly IEmailPoolService emailPoolService;
        private readonly IMappingEngine mapper;
        //
        // GET: /Compose/

        public ComposeController(IViewModelBuilder<ComposeViewModel> indexViewModelBuilder, IEmailRepository emailRepository, IEmailPoolService emailPoolService, IMappingEngine mapper)
        {
            this.indexViewModelBuilder = indexViewModelBuilder;
            this.emailRepository = emailRepository;
            this.emailPoolService = emailPoolService;
            this.mapper = mapper;
        }

        public ActionResult Index()
        {
            var viewModel = indexViewModelBuilder.Build();
            return View(viewModel);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Index(ComposeModel composeModel)
        {
            var email = mapper.Map<ComposeModel, Email>(composeModel);
            emailRepository.Store(email);
            emailPoolService.AddEmail(email);
            return null;
        }

    }
}
