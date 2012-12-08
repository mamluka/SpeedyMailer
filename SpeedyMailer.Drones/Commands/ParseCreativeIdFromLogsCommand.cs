using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Drones.Tasks;

namespace SpeedyMailer.Drones.Commands
{
    public class ParseCreativeIdFromLogsCommand : Command<IList<MailIdCreativeIdMap>>
    {
        public IList<MailLogEntry> Logs { get; set; }

        public override IList<MailIdCreativeIdMap> Execute()
        {
            return Logs
                .Select(x => new MailIdCreativeIdMap
                    {
                        MessageId = ParseRegexWithOneGroup(x.msg, @"^\s?(.+?):\s"),
                        CreativeId = ParseRegexWithOneGroup(x.msg, @"Speedy-Creative-Id:\s(.+?)\s"),
                    })
                    .Where(x => !string.IsNullOrEmpty(x.CreativeId))
                    .ToList();
        }

        private string ParseRegexWithMiltipleGroup(string msg, string pattern, int groupId)
        {
            var match = Regex.Match(msg, pattern);
            return match.Success ? match.Groups[groupId].Value : "";
        }

        private string ParseRegexWithOneGroup(string msg, string pattern)
        {
            return ParseRegexWithMiltipleGroup(msg, pattern, 1);
        }
    }
}