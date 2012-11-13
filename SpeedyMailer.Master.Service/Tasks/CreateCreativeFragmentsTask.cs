using System;
using System.Collections.Generic;
using Raven.Client;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Creative;
using System.Linq;
using SpeedyMailer.Core.Rules;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Master.Service.Storage.Indexes;
using Raven.Client.Linq;

namespace SpeedyMailer.Master.Service.Tasks
{
	public class CreateCreativeFragmentsTask : PersistentTask
	{
		public string CreativeId { get; set; }
	}

	public class CreateCreativeFragmentsTaskExecutor : PersistentTaskExecutor<CreateCreativeFragmentsTask>
	{
		private readonly IDocumentStore _documentStore;
		private readonly CreativeEndpointsSettings _creativeEndpointsSettings;
		private readonly ServiceSettings _serviceSettings;
		private readonly CreativeFragmentSettings _creativeFragmentSettings;

		public CreateCreativeFragmentsTaskExecutor(IDocumentStore documentStore, CreativeEndpointsSettings creativeEndpointsSettings, ServiceSettings serviceSettings, CreativeFragmentSettings creativeFragmentSettings)
		{
			_creativeFragmentSettings = creativeFragmentSettings;
			_serviceSettings = serviceSettings;
			_creativeEndpointsSettings = creativeEndpointsSettings;
			_documentStore = documentStore;
		}

		public override void Execute(CreateCreativeFragmentsTask task)
		{
			using (var session = _documentStore.OpenSession())
			{
				var intervalRules = session.Query<IntervalRule>().ToList();
				var creative = session.Load<Creative>(task.CreativeId);

				var unsubscribeTempalte = session.Load<Template>(creative.UnsubscribeTemplateId);
				var listId = creative.Lists.First();

				var domainGroups = GetDomainGroups(intervalRules);
				var domainGroupsTotal = GetDomainGroupTotals(session, domainGroups, listId);

				var totalContacts = domainGroupsTotal.Sum(x => x.Value);

				var recipientsPerFragment = _creativeFragmentSettings.RecipientsPerFragment;
				var leftoverFragmentSize = totalContacts % recipientsPerFragment;
				var numberOfFullSizedFragments = (totalContacts - leftoverFragmentSize) / recipientsPerFragment;

				var groupsTotal = new List<GroupSummery>();
				groupsTotal = PopulateGroupSummeryWithTotals(intervalRules, groupsTotal, domainGroupsTotal);

				CalculateFragmentDistribution(leftoverFragmentSize, totalContacts, groupsTotal, numberOfFullSizedFragments);

				var groupToTakeFrom = GetTheExtraGroupPerFragmentDistribution(groupsTotal, numberOfFullSizedFragments);

				var extraSkipCounter = domainGroups.ToDictionary(x => x, y => 0);
				extraSkipCounter[_creativeFragmentSettings.DefaultGroup] = 0;

				var totalNumberOfFragments = GetTotalNumberOfFragments(leftoverFragmentSize, numberOfFullSizedFragments);

				for (int i = 0; i < totalNumberOfFragments; i++)
				{
					var contacts = new List<Contact>();

					foreach (var groupSummery in groupsTotal)
					{
						var howManyAdditionalContactsToTake = GetHowManyAdditionalContactsToTake(groupToTakeFrom, i, groupSummery);

						var currentFragmentGroupContacts = session.Query<Contact>()
							.Customize(x => x.WaitForNonStaleResults())
							.Where(contact => contact.MemberOf.Any(x => x == listId) && contact.DomainGroup == groupSummery.Group)
							.Skip(i * groupSummery.RegularFragmentChunkSize + extraSkipCounter[groupSummery.Group])
							.Take(groupSummery.RegularFragmentChunkSize + howManyAdditionalContactsToTake)
							.ToList();

						extraSkipCounter[groupSummery.Group] += howManyAdditionalContactsToTake;

						contacts = contacts.Union(currentFragmentGroupContacts).ToList();
					}

					var recipients = contacts.Select(ToRecipient).ToList();
					ApplyDefaultRules(recipients);
					ApplyIntervalRules(recipients);

					var fragment = new CreativeFragment
									   {
										   Body = creative.Body,
										   CreativeId = creative.Id,
										   Subject = creative.Subject,
										   Recipients = recipients,
										   UnsubscribeTemplate = unsubscribeTempalte.Body,
										   FromAddressDomainPrefix = creative.FromAddressDomainPrefix,
										   FromName = creative.FromName,
										   Service = new Core.Domain.Master.Service
														 {
															 BaseUrl = _serviceSettings.BaseUrl,
															 DealsEndpoint = _creativeEndpointsSettings.Deal,
															 UnsubscribeEndpoint = _creativeEndpointsSettings.Unsubscribe
														 }
									   };
					session.Store(fragment);
					session.SaveChanges();
				}
			}
		}

		private static int GetHowManyAdditionalContactsToTake(List<Dictionary<string, int>> groupToTakeFrom, int i, GroupSummery groupSummery)
		{
			if (i + 1 > groupToTakeFrom.Count)
				return 0;

			return (groupToTakeFrom[i].ContainsKey(groupSummery.Group) ? 1 : 0);
		}

		private static int GetTotalNumberOfFragments(int leftoverFragmentSize, int numberOfFullSizedFragments)
		{
			return leftoverFragmentSize == 0 ? numberOfFullSizedFragments : numberOfFullSizedFragments + 1;
		}

		private static List<Dictionary<string, int>> GetTheExtraGroupPerFragmentDistribution(IEnumerable<GroupSummery> groupsTotal, int numberOfFullSizedFragments)
		{
			var theExtraGroupPerFragmentDistribution = groupsTotal.SelectMany(x => Enumerable.Range(1, x.RegularFragmentChunkLeftOver).Select(i => x.Group)).Select((x, i) => new { i, x }).GroupBy(x => x.i % numberOfFullSizedFragments).Select(x => x.ToDictionary(key => key.x, y => 0)).ToList();

			return theExtraGroupPerFragmentDistribution;
		}

		private static void CalculateFragmentDistribution(int leftoverFragmentSize, int totalContacts, List<GroupSummery> groupsTotal, int numberOfFullSizedFragments)
		{
			var multiplier = (decimal)leftoverFragmentSize / totalContacts;

			if (ThereIsALeftOverFragmentAsTheLastFragment(multiplier))
			{
				groupsTotal.ForEach(x =>
				{
					x.ContributionToLeftOver = (int)Math.Floor((decimal)(multiplier * x.Total));
					x.TotalWithOutLeftOver = x.Total - x.ContributionToLeftOver;
					x.RegularFragmentChunkLeftOver = x.TotalWithOutLeftOver % numberOfFullSizedFragments;
					x.RegularFragmentChunkSize = (x.TotalWithOutLeftOver - x.RegularFragmentChunkLeftOver) / numberOfFullSizedFragments;
				});

				var complitionToCorrentSize = leftoverFragmentSize - groupsTotal.Sum(x => x.ContributionToLeftOver);

				var lastGroupTotal = groupsTotal.Last();
				lastGroupTotal.ContributionToLeftOver += complitionToCorrentSize;
				lastGroupTotal.TotalWithOutLeftOver -= complitionToCorrentSize;
				lastGroupTotal.RegularFragmentChunkLeftOver = lastGroupTotal.TotalWithOutLeftOver % numberOfFullSizedFragments;
				lastGroupTotal.RegularFragmentChunkSize = (lastGroupTotal.TotalWithOutLeftOver - lastGroupTotal.RegularFragmentChunkLeftOver) / numberOfFullSizedFragments;
			}
			else
			{
				groupsTotal.ForEach(x =>
										{
											x.RegularFragmentChunkSize = x.Total;
										});
			}


		}

		private static bool ThereIsALeftOverFragmentAsTheLastFragment(decimal multiplier)
		{
			return multiplier < 1;
		}

		private List<GroupSummery> PopulateGroupSummeryWithTotals(IEnumerable<IntervalRule> intervalRules, List<GroupSummery> groupsTotal, Dictionary<string, int> domainGroupsTotal)
		{
			groupsTotal
				.AddRange(intervalRules
					          .Select(intervalRule => new GroupSummery
						                                  {
							                                  Group = intervalRule.Group,
							                                  Total = domainGroupsTotal[intervalRule.Group],
							                                  Conditions = intervalRule.Conditons,
							                                  Interval = intervalRule.Interval
						                                  }));

			groupsTotal.Add(new GroupSummery
								{
									Group = _creativeFragmentSettings.DefaultGroup,
									Total = domainGroupsTotal[_creativeFragmentSettings.DefaultGroup],
									Interval = _creativeFragmentSettings.DefaultInterval
								});

			groupsTotal = groupsTotal.OrderByDescending(x => x.Interval).ToList();
			return groupsTotal;
		}

		private static Dictionary<string, int> GetDomainGroupTotals(IDocumentSession session, IEnumerable<string> domainGroups, string listId)
		{
			return domainGroups
				.Select(
					domainGroup =>
					session.Query<Contacts_DomainGroupCounter.ReduceResult, Contacts_DomainGroupCounter>()
						.Customize(x => x.WaitForNonStaleResults(TimeSpan.FromMinutes(5))).SingleOrDefault(x => x.DomainGroup == domainGroup && x.ListId == listId))
				.Where(x => x != null)
				.ToDictionary(x => x.DomainGroup, y => y.Count);
		}

		private IList<string> GetDomainGroups(IEnumerable<IntervalRule> intervalRules)
		{
			var domainGroups = intervalRules.Select(rule => rule.Group).ToList();
			domainGroups.Add(_creativeFragmentSettings.DefaultGroup);
			return domainGroups;
		}

		public class GroupSummery
		{
			public string Group { get; set; }
			public int Total { get; set; }
			public int ChunkSize { get; set; }
			public int Interval { get; set; }
			public int ContributionToLeftOver { get; set; }
			public int RegularFragmentChunkLeftOver { get; set; }
			public int RegularFragmentChunkSize { get; set; }
			public int TotalWithOutLeftOver { get; set; }

			public List<string> Conditions { get; set; }
		}

		private Recipient ToRecipient(Contact contact)
		{
			return new Recipient
						   {
							   Email = contact.Email,
							   Name = contact.Name,
							   ContactId = contact.Id
						   };
		}

		private void ApplyDefaultRules(IEnumerable<Recipient> recipients)
		{
			recipients
				.ToList()
				.ForEach(x =>
							 {
								 x.Interval = _creativeFragmentSettings.DefaultInterval;
								 x.Group = _creativeFragmentSettings.DefaultGroup;
							 });
		}

		private void ApplyIntervalRules(IEnumerable<Recipient> recipients)
		{
			using (var session = _documentStore.OpenSession())
			{
				var rules = session.Query<IntervalRule>().ToList();
				var matchingConditionsActions = rules.SelectMany(x => x.Conditons.Select(condition => new { Condition = condition, x.Interval, x.Group })).ToList();

				recipients
					.ToList()
					.ForEach(x => matchingConditionsActions
									  .Where(condition => x.Email.Contains(condition.Condition))
									  .ToList()
									  .ForEach(action =>
												   {
													   x.Interval = action.Interval;
													   x.Group = action.Group;
												   }));
			}
		}
	}
}