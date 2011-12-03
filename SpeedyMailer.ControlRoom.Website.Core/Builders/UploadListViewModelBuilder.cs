using SpeedyMailer.ControlRoom.Website.Core.ViewModels;
using SpeedyMailer.Core.Lists;

namespace SpeedyMailer.ControlRoom.Website.Core.Builders
{
    public class UploadListViewModelBuilder:IViewModelBuilder<UploadListViewModel>
    {
        private readonly IListRepository listRepository;

        public UploadListViewModelBuilder(IListRepository listRepository)
        {
            this.listRepository = listRepository;
        }

        public UploadListViewModel Build()
        {
            var listCollection = listRepository.Lists();

            return new UploadListViewModel()
                       {
                           Lists = listCollection.Lists
                       };
        }
    }
}