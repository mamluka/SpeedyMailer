using SpeedyMailer.Core.DataAccess.Lists;
using SpeedyMailer.Master.Web.Core.ViewModels;

namespace SpeedyMailer.Master.Web.Core.Builders
{
    public class UploadListViewModelBuilder : IViewModelBuilder<UploadListViewModel>
    {
        private readonly IListRepository listRepository;

        public UploadListViewModelBuilder(IListRepository listRepository)
        {
            this.listRepository = listRepository;
        }

        #region IViewModelBuilder<UploadListViewModel> Members

        public UploadListViewModel Build()
        {
            ListsStore listCollection = listRepository.Lists();

            return new UploadListViewModel
                       {
                           Lists = listCollection.Lists
                       };
        }

        #endregion
    }
}