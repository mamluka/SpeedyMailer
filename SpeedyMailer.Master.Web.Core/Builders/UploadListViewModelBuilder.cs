using SpeedyMailer.Domain.DataAccess.Lists;
using SpeedyMailer.Master.Web.Core.ViewModels;

namespace SpeedyMailer.Master.Web.Core.Builders
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