using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpeedyMailer.ControlRoom.Website.ViewModels.Builders;
using Rhino.Mocks;

namespace SpeedyMailer.ControlRoom.Website.Tests
{
    public static class Extentions
    {
        public static void MockBuild<T>(this IViewModelBuilder<T> builder) where T : new()
        {
            builder.Expect(x => x.Build()).Repeat.Once();
        }
    }
}
