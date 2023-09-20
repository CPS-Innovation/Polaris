using System.Text.RegularExpressions;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.OperationalInsights;

namespace polaris_analytics
{
  internal class Program
  {
    const string ProdSubscriptionId = "/subscriptions/888d158f-ade3-4528-9c59-84bd2260d5ee";
    const string LogAnalyticsResourceGroupName = "rg-polaris-analytics";
    const string LogAnalyticsResourceName = "la-polaris";

    const string EmailRegexPattern = @"[^@\s""']+@[^@\s]+\.[^@\s""']+";

    static void Main()
    {
      var client = new ArmClient(new DefaultAzureCredential());

      var subscriptionIdentifier = new ResourceIdentifier(ProdSubscriptionId);
      var subscription = client.GetSubscriptionResource(subscriptionIdentifier);

      var resourceGroups = subscription.GetResourceGroups();
      var resourceGroup = resourceGroups.Get(LogAnalyticsResourceGroupName).Value;

      var workspace = resourceGroup.GetOperationalInsightsWorkspace(LogAnalyticsResourceName).Value;
      var polarisSavedSearches = workspace.GetOperationalInsightsSavedSearches()
          .Where(savedSearch => savedSearch.Id.Name.Contains("polaris_", StringComparison.OrdinalIgnoreCase))
          .ToList();

      foreach (var polarisSavedSearch in polarisSavedSearches)
      {
        var fileName = Path.ChangeExtension(polarisSavedSearch.Data.DisplayName, ".kql");
        var filePath = Path.Join("../queries", fileName);

        // remove email address
        var redactedQuery = Regex.Replace(polarisSavedSearch.Data.Query, EmailRegexPattern, "REDACTED@REDACTED.EMAIL.ADDRESS");
        File.WriteAllText(filePath, redactedQuery);
      }
    }
  }
}

