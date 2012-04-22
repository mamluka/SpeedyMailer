using System.Web.Mvc;
using AutoMapper;
using SpeedyMailer.Core.DataAccess.Emails;
using SpeedyMailer.Core.Domain.Emails;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.Master.Web.Core.Builders;
using SpeedyMailer.Master.Web.Core.Models;
using SpeedyMailer.Master.Web.Core.ViewModels;

namespace SpeedyMailer.Master.Web.UI.Controllers
{
    public class ComposerController : Controller
    {
        private readonly IEmailPoolService emailPoolService;
        private readonly IEmailRepository emailRepository;
        private readonly IViewModelBuilder<ComposeViewModel> indexViewModelBuilder;
        private readonly IMappingEngine mapper;
        //
        // GET: /Compose/

        public ComposerController(IViewModelBuilder<ComposeViewModel> indexViewModelBuilder,
                                 IEmailRepository emailRepository, IEmailPoolService emailPoolService,
                                 IMappingEngine mapper)
        {
            this.indexViewModelBuilder = indexViewModelBuilder;
            this.emailRepository = emailRepository;
            this.emailPoolService = emailPoolService;
            this.mapper = mapper;
        }

        public ActionResult Index()
        {
            ComposeViewModel viewModel = indexViewModelBuilder.Build();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Index(ComposeModel composeModel)
        {
            Email email = mapper.Map<ComposeModel, Email>(composeModel);
            emailRepository.Store(email);
            emailPoolService.AddEmail(email);
            return null;
        }
    }
}