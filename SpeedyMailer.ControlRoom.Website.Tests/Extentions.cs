using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpeedyMailer.ControlRoom.Website.Core.ViewModels.Builders;
using Rhino.Mocks;

namespace SpeedyMailer.ControlRoom.Website.Tests
{
    public static class Extentions
    {
        public static void ExpectBuild<TViewModel, TParameter>(this IViewModelBuilderWithBuildParameters<TViewModel, TParameter> builder)
        {
            builder.Expect(x => x.Build(Arg<TParameter>.Is.Anything)).Repeat.Once();
        }
    }
}
