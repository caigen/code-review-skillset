using Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace CommonTests
{
    /// <summary>
    /// Unit tests for PullRequestHelper
    /// </summary>
    [TestClass]
    public class PullRequestHelperTests
    {
        private PullRequestHelper? _pullRequestHelper;
        private const string TestOrganizationUrl = "https://dev.azure.com/yourorganization";
        private const string TestPersonalAccessToken = "your-pat-token";

        [TestInitialize]
        public void TestInitialize()
        {
            // Initialize with test Azure DevOps organization URL and Personal Access Token
            // For integration tests, use environment variable if available
            var pat = Environment.GetEnvironmentVariable("AZURE_DEVOPS_PAT") ?? TestPersonalAccessToken;
            _pullRequestHelper = new PullRequestHelper(TestOrganizationUrl, pat);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _pullRequestHelper?.Dispose();
        }

        /// <summary>
        /// Integration test example - would require actual Azure DevOps setup
        /// </summary>
        [TestMethod]
        [TestCategory("Integration")]
        public async Task IntegrationTest_GetPullRequestAsync_WithRealConnection()
        {
            // This test would be used in an integration test environment
            // with actual Azure DevOps organization, project, and pull request IDs
            var realOrganizationUrl = "https://dev.azure.com/skype";
            var realPat = Environment.GetEnvironmentVariable("AZURE_DEVOPS_PAT");
            var realProjectName = "SCC";
            var realRepositoryId = "sync_calling_concore-conversation"; // Replace with actual repository ID or name

            if (string.IsNullOrEmpty(realPat))
            {
                Assert.Inconclusive("AZURE_DEVOPS_PAT environment variable not set");
                return;
            }

            using var realPullRequestHelper = new PullRequestHelper(realOrganizationUrl, realPat);

            List<int> pullRequestIds = new List<int>
            {
                1217873, // Example pull request ID
                1222954  // Another example pull request ID
            };

            // Skip test if no pull request IDs are provided
            if (!pullRequestIds.Any())
            {
                Assert.Inconclusive("No pull request IDs provided for testing");
                return;
            }

            // Loop through each pull request ID to get details
            foreach (var prId in pullRequestIds)
            {
                var pullRequest = await realPullRequestHelper.GetPullRequestAsync(realProjectName, realRepositoryId, prId);

                Console.WriteLine($"Pull Request Details for {prId}: {JsonConvert.SerializeObject(pullRequest, Formatting.Indented)}");

                // Assert
                Assert.IsNotNull(pullRequest);
                Assert.AreEqual(prId, pullRequest.PullRequestId);
                Assert.IsNotNull(pullRequest.Title);
                Assert.IsNotNull(pullRequest.SourceRefName);
                Assert.IsNotNull(pullRequest.TargetRefName);
                Assert.IsNotNull(pullRequest.Status);
                
                Console.WriteLine($"PR {pullRequest.PullRequestId} - Title: {pullRequest.Title}");
                Console.WriteLine($"Source: {pullRequest.SourceRefName} -> Target: {pullRequest.TargetRefName}");
                Console.WriteLine($"Status: {pullRequest.Status}, Created By: {pullRequest.CreatedBy?.DisplayName}");
            }
        }
    }
}
