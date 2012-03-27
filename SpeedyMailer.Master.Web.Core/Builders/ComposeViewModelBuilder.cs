using System.Collections.Generic;
using AutoMapper;
using SpeedyMailer.Domain.DataAccess.Lists;
using SpeedyMailer.Domain.Model.Lists;
using SpeedyMailer.Master.Web.Core.ComponentViewModel;
using SpeedyMailer.Master.Web.Core.ViewModels;

namespace SpeedyMailer.Master.Web.Core.Builders
{
    public class ComposeViewModelBuilder : IViewModelBuilder<ComposeViewModel>
    {
        private readonly IListRepository listRepository;
        private readonly IMappingEngine mapper;

        public ComposeViewModelBuilder(IListRepository listRepository, IMappingEngine mapper)
        {
            this.listRepository = listRepository;
            this.mapper = mapper;
        }

        public ComposeViewModel Build()
        {
            var viewModel = new ComposeViewModel();
            var listCollection = mapper.Map<List<ListDescriptor>, List<ListDescriptorViewModel>>(listRepository.Lists().Lists);

            viewModel.AvailableLists = listCollection;

            return viewModel;
        }
    }
}