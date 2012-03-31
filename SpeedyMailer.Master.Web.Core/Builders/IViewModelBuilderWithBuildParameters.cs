namespace SpeedyMailer.Master.Web.Core.Builders
{
    public interface IViewModelBuilderWithBuildParameters<out TViewModel, in TParameter>
    {
        TViewModel Build(TParameter parameter);
    }
}