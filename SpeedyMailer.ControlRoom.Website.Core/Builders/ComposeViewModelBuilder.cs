using System.Collections.Generic;
using AutoMapper;
using SpeedyMailer.ControlRoom.Website.Core.ComponentViewModel;
using SpeedyMailer.ControlRoom.Website.Core.ViewModels;
using SpeedyMailer.Core.Lists;

namespace SpeedyMailer.ControlRoom.Website.Core.Builders
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