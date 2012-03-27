using Rhino.Mocks;
using SpeedyMailer.Master.Web.Core.Builders;

namespace SpeedyMailer.Master.Web.Tests
{
    public static class Extentions
    {
        public static void ExpectBuild<TViewModel, TParameter>(this IViewModelBuilderWithBuildParameters<TViewModel, TParameter> builder)
        {
            builder.Expect(x => x.Build(Arg<TParameter>.Is.Anything)).Repeat.Once();
        }

        public static void ExpectBuild<TViewModel>(this IViewModelBuilder<TViewModel> builder) where TViewModel : class ,new()
        {
            builder.Expect(x => x.Build()).Repeat.Once();
        }
    }
}
