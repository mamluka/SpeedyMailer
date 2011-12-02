namespace SpeedyMailer.ControlRoom.Website.Core.Builders
{
    public interface IViewModelBuilder<T> where T : new()
    {
        T Build();
    }

    public interface IViewModelBuilderWithBuildParameters<out TViewModel, in TParameter>
    {
        TViewModel Build(TParameter parameter);
    }
}