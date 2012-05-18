using Nancy.ModelBinding;
using SpeedyMailer.Core.Protocol;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Master.Service.Commands;

namespace SpeedyMailer.Master.Service.Api
{
    public class CreativeModule:ModuleBase
    {
        public CreativeModule(CreateCreativeFragmentsCommand createCreativeFragmentsCommand,ICreativeFragmentSettings creativeFragmentSettings):base("/creative")
        {
            Post["/add"] = call =>
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
