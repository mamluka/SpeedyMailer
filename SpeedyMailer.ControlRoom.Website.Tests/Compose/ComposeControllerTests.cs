using System;
using System.Linq;
using System.Text;
using NUnit;
using FluentAssertions;
using NUnit.Framework;
using Rhino.Mocks;
using SpeedyMailer.ControlRoom.Website.Controllers;
using SpeedyMailer.ControlRoom.Website.Core.ViewModels;
using SpeedyMailer.ControlRoom.Website.Tests.Maps;
using SpeedyMailer.Tests.Core;
using MvcContrib.TestHelper;

namespace SpeedyMailer.ControlRoom.Website.Tests.Compose
{
    class ComposeControllerTests : AutoMapperAndFixtureBase<AutoMapperMaps>
    {
        [Test]
        public void Index_ShouldRenderTheRightViewModel()
        {
            //Arrange
            var controller = new ComposeController();
            //Act
            var viewModel = controller.Index();
            //Assert
            viewModel.AssertViewRendered().WithViewData<ComposeViewModel>();
        }
    }
}
