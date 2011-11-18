namespace SpeedyMailer.ControlRoom.Website.ViewModels.Builders
{
    public interface IViewModelBuilder<T> where T : new()
    {
        T Build();
    }
}