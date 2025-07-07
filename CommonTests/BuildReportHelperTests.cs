using Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace CommonTests
{
    /// <summary>
    /// Unit tests for BuildReportHelper
    /// </summary>
    [TestClass]
    public class BuildReportHelperTests
    {
        private BuildReportHelper? _buildReportHelper;
        private const string TestOrganizationUrl = "https://dev.azure.com/yourorganization";
        private const string TestPersonalAccessToken = "your-pat-token";

        [TestInitialize]
        public void TestInitialize()
        {
            // Initialize with test Azure DevOps organization URL and Personal Access Token
            // For integration tests, use environment variable if available
            var pat = Environment.GetEnvironmentVariable("AZURE_DEVOPS_PAT") ?? TestPersonalAccessToken;
            _buildReportHelper = new BuildReportHelper(TestOrganizationUrl, pat);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _buildReportHelper?.Dispose();
        }

        /// <summary>
        /// Integration test example - would require actual Azure DevOps setup
        /// </summary>
        [TestMethod]
        [TestCategory("Integration")]
        public async Task IntegrationTest_GetBuildReportAsync_WithRealConnection()
        {
            // This test would be used in an integration test environment
            // with actual Azure DevOps organization, project, and build IDs
            var realOrganizationUrl = "https://dev.azure.com/skype";
            var realPat = Environment.GetEnvironmentVariable("AZURE_DEVOPS_PAT");
            var realProjectName = "SCC";

            if (string.IsNullOrEmpty(realPat))
            {
                Assert.Inconclusive("AZURE_DEVOPS_PAT environment variable not set");
                return;
            }

            using var realBuildReportHelper = new BuildReportHelper(realOrganizationUrl, realPat);

            List<string> buildId = new List<string>
            {
                //"71015951", // Example build ID
                "70997686"  // one release pipeline;
            };

            // Loop through each build ID to get reports
            foreach (var build in buildId)
            {
                var report = await realBuildReportHelper.GetBuildReportAsync(realProjectName, int.Parse(build));

                Console.WriteLine($"Build Report for {build}: {JsonConvert.SerializeObject(report)}");

                // Assert
                Assert.IsNotNull(report);
                Assert.AreEqual(int.Parse(build), report.BuildId);
                Assert.AreEqual(realProjectName, report.ProjectName);
                Assert.IsNotNull(report.BuildNumber);
                Assert.IsTrue(report.Duration >= TimeSpan.Zero, "Build duration should be non-negative");
                
                Console.WriteLine($"Build {report.BuildNumber} - Status: {report.Status}, Result: {report.Result}");
            }
        }
    }
}
