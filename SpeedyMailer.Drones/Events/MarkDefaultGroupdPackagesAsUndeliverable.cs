using System;
using System.Linq;
using System.Text.RegularExpressions;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Evens;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Drones.Commands;
using SpeedyMailer.Drones.Storage;

namespace SpeedyMailer.Drones.Events
{
    public class MarkDefaultGroupdPackagesAsUndeliverable : IHappendOn<AggregatedMailBounced>, IHappendOn<AggregatedMailDeferred>
    {
        private readonly ClassifyNonDeliveredMailCommand _classifyNonDeliveredMailCommand;
        private readonly CreativeFragmentSettings _creativeFragmentSettings;
        private readonly CreativePackagesStore _creativePackagesStore;

        public MarkDefaultGroupdPackagesAsUndeliverable(ClassifyNonDeliveredMailCommand classifyNonDeliveredMailCommand, CreativePackagesStore creativePackagesStore, CreativeFragmentSettings creativeFragmentSettings)
        {
            _creativePackagesStore = creativePackagesStore;
            _creativeFragmentSettings = creativeFragmentSettings;
            _classifyNonDeliveredMailCommand = classifyNonDeliveredMailCommand;
        }

        public void Inspect(AggregatedMailBounced data)
        {
            UndeliverabilityDecision(data);
        }

        public void Inspect(AggregatedMailDeferred data)
        {
            UndeliverabilityDecision(data);
        }

        private void UndeliverabilityDecision<T>(AggregatedMailEvents<T> data) where T : IHasDomainGroup, IHasRecipient, IHasRelayMessage
        {
            var domainToUndeliver = data
                .MailEvents
                .Where(x => x.DomainGroup == _creativeFragmentSettings.DefaultGroup)
                .Select(x =>
                    {
                        _classifyNonDeliveredMailCommand.Message = x.Message;
                        return new { Classification = _classifyNonDeliveredMailCommand.Execute(), x.Recipient };
                    })
                .Where(x => x.Classification.BounceType == BounceType.Blocked)
                .Select(x => new { ExtendedClassification = x, Domain = GetDomain(x.Recipient) })
                .Where(x => !string.IsNullOrEmpty(x.Domain))
                .GroupBy(x => x.Domain)
                .ToList();

            var defaultGroupPackages = _creativePackagesStore.GetUnprocessedDefaultGroupPackages();

            var defaultPackagesDomains = defaultGroupPackages
                .GroupBy(x => GetDomain(x.To));

            var packagesToUndeliverGroupedByDomains = domainToUndeliver.Join(defaultPackagesDomains, x => x.Key, y => y.Key, (x, y) => new
                {
                    PackagesFromSameDoman = y.ToList(),
                    Time = x.First().ExtendedClassification.Classification.TimeSpan
                }).ToList();

            packagesToUndeliverGroupedByDomains.ForEach(x => x.PackagesFromSameDoman.ForEach(p =>
                {
                    p.Processed = true;
                    _creativePackagesStore.Save(p);
                }));
        }

        private string GetDomain(string to)
        {
            return Regex.Match(to, "@(.+?)$").Groups[0].Value;
        }
    }
}