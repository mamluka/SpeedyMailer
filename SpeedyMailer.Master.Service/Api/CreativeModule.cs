using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Nancy.ModelBinding;
using SpeedyMailer.Core.Protocol;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Master.Service.Commands;

namespace SpeedyMailer.Master.Service.Api
{
    public class CreativeModule:ModuleBase
    {
        public CreativeModule(IKernel kernel,CreateCreativeFragmentsCommand createCreativeFragmentsCommand,ICreativeFragmentSettings creativeFragmentSettings):base("/creative")
        {
            Get["/add"] = call =>
                              {
                                  var model = this.Bind<CreativeApi.Add.Request>();
                                  ExecuteCommand(createCreativeFragmentsCommand, x=>
                                                                                     {
                                                                                         x.CreativeId = model.CreativeId;
                                                                                         x.RecipientsPerFragment = creativeFragmentSettings.RecipientsPerFragment;
                                                                                     });
                                   return "sdfsdf";
                               };
        }
    }
}
