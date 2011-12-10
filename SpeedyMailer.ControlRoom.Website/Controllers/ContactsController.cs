using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using SpeedyMailer.ControlRoom.Website.Core.Builders;
using SpeedyMailer.ControlRoom.Website.Core.Models;
using SpeedyMailer.ControlRoom.Website.Core.ViewModels;
using SpeedyMailer.Core.Contacts;
using SpeedyMailer.Core.Lists;

namespace SpeedyMailer.ControlRoom.Website.Controllers
{
    public class ContactsController : Controller
    {
        private readonly IContactsCSVParser contactsCSVParser;
        private readonly IViewModelBuilderWithBuildParameters<UploadListViewModel, IContactsCSVParser> contactslUploadViewModelBuilder;
        private readonly IViewModelBuilder<UploadListViewModel> uploadListViewModelBuilder;
        private readonly IMappingEngine mapper;

        public ContactsController(IContactsCSVParser contactsCSVParser, IViewModelBuilderWithBuildParameters<UploadListViewModel, IContactsCSVParser> contactslUploadViewModelBuilder, IViewModelBuilder<UploadListViewModel> uploadListViewModelBuilder, IMappingEngine mapper)
        {
            this.contactsCSVParser = contactsCSVParser;
            this.contactslUploadViewModelBuilder = contactslUploadViewModelBuilder;
            this.uploadListViewModelBuilder = uploadListViewModelBuilder;
            this.mapper = mapper;
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
            return View(viewModel);
        }
        [HttpPost]
        public ActionResult Upload(UploadListModel model)
        {
            var initModel = mapper.Map<UploadListModel, InitialContactsBatchOptions>(model);
            contactsCSVParser.AddInitialContactBatchOptions(initModel);
            contactsCSVParser.ParseAndStore();
            var viewModel = contactslUploadViewModelBuilder.Build(contactsCSVParser);
            return View(viewModel);
        }

    }
}
