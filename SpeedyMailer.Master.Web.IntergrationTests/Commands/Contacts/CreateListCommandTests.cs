using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Rhino.Mocks;
using FluentAssertions;
using SpeedyMailer.Master.Web.Core.Commands;
using SpeedyMailer.Tests.Acceptance.Specs.Drone;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Master.Web.IntergrationTests.Commands.Contacts
{
    [TestFixture]
    public class CreateListCommandTests : IntegrationTestBase
    {
        [SetUp]
        public void Setup()
        {
            
        }

        [Test]
        public void Execute_WhenCalled_ShouldCreateAList()
        {
            var result = Master.ExecuteCommand<CreateListCommand, string>(x=>
                                                                 {
                                                                     x.Id = 1;
                                                                     x.Name = "Default";
                                                                 });
            
        }

        [Test]
        public void Method_WhenSomething_ShouldStory()
        {
            var a = new A()
                        {
                            One = "sex",
                            Two = 123

                        };

            var b = new A()
                        {
                            Two = 123,
                            One = "sex"
                        };

            Compare(a, b).Should().BeTrue();
        }
    }

    public class A
    {
        public string One { get; set; }
        public int Two { get; set; }
    }
}
