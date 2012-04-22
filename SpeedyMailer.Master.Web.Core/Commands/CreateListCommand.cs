using SpeedyMailer.Core.Commands;

namespace SpeedyMailer.Master.Web.Core.Commands
{
    public class CreateListCommand : Command<string>
    {
        public string Name { get; set; }
        public int Id { get; set; }

        public override string Execute()
        {
            return null;
        }
    }
}