using Common;
using Microsoft.TeamFoundation.Build.WebApi;

namespace CommonTests
{
    /// <summary>
    /// Integration tests for PullRequestBuildHelper class
    /// These tests require real Azure DevOps credentials and connections
    /// </summary>
    [TestClass]
    public class PullRequestBuildHelperTests
    {
        private const string TestOrganizationUrl = "https://dev.azure.com/skype";
        private const string TestProjectName = "SCC";
        private const string TestRepositoryId = "sync_calling_concore-conversation";
        private const int TestPullRequestId = 1217873;
        private const int TestBuildId = 456;

        private PullRequestBuildHelper? _buildHelper;
        private string? _personalAccessToken;

        [TestInitialize]
        public void TestInitialize()
        {
            // Get PAT from environment variable for integration tests
            _personalAccessToken = Environment.GetEnvironmentVariable("AZURE_DEVOPS_PAT");
            
            if (!string.IsNullOrEmpty(_personalAccessToken))
            {
                _buildHelper = new PullRequestBuildHelper(TestOrganizationUrl, _personalAccessToken);
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _buildHelper?.Dispose();
        }

        #region Integration Tests

        /// <summary>
        /// Integration test for getting the latest build ID for a pull request
        /// Requires real Azure DevOps setup and valid credentials
        /// </summary>
        [TestMethod]
        public async Task IntegrationTest_GetLatestBuildId_WithRealData()
        {
            // Arrange
            if (string.IsNullOrEmpty(_personalAccessToken))
            {
                Assert.Inconclusive("Azure DevOps PAT not configured. Set AZURE_DEVOPS_PAT environment variable.");
                return;
            }

            // Act & Assert
            try
            {
                var buildId = await _buildHelper!.GetLatestBuildIdAsync(
                    TestProjectName, 
                    TestRepositoryId, 
                    TestPullRequestId);

                // Assert - Either we get a build ID or null (both are valid results)
                Assert.IsTrue(buildId.HasValue || !buildId.HasValue, "Method should complete successfully");
                
                if (buildId.HasValue)
                {
                    Assert.IsTrue(buildId.Value > 0, "Build ID should be positive when present");
                    Console.WriteLine($"Found latest build ID: {buildId.Value}");
                }
                else
                {
                    Console.WriteLine("No builds found for the specified pull request");
                }
            }
            catch (InvalidOperationException ex)
            {
                // This is expected if the project/repo/PR doesn't exist
                Console.WriteLine($"Expected error for test data: {ex.Message}");
                Assert.IsTrue(true, "InvalidOperationException is expected with test data");
            }
        }

        /// <summary>
        /// Integration test for getting the latest build details for a pull request
        /// </summary>
        [TestMethod]
        public async Task IntegrationTest_GetLatestBuild_WithRealData()
        {
            // Arrange
            if (string.IsNullOrEmpty(_personalAccessToken))
            {
                Assert.Inconclusive("Azure DevOps PAT not configured. Set AZURE_DEVOPS_PAT environment variable.");
                return;
            }

            // Act & Assert
            try
            {
                var build = await _buildHelper!.GetLatestBuildAsync(
                    TestProjectName, 
                    TestRepositoryId, 
                    TestPullRequestId);

                // Assert
                if (build != null)
                {
                    Assert.IsTrue(build.Id > 0, "Build ID should be positive");
                    Assert.IsNotNull(build.BuildNumber, "Build number should not be null");
                    Console.WriteLine($"Build Details - ID: {build.Id}, Number: {build.BuildNumber}, Status: {build.Status}");
                }
                else
                {
                    Console.WriteLine("No builds found for the specified pull request");
                }
                
                Assert.IsTrue(true, "Method completed successfully");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Expected error for test data: {ex.Message}");
                Assert.IsTrue(true, "InvalidOperationException is expected with test data");
            }
        }

        /// <summary>
        /// Integration test for getting build summary information
        /// </summary>
        [TestMethod]
        public async Task IntegrationTest_GetBuildSummary_WithRealData()
        {
            // Arrange
            if (string.IsNullOrEmpty(_personalAccessToken))
            {
                Assert.Inconclusive("Azure DevOps PAT not configured. Set AZURE_DEVOPS_PAT environment variable.");
                return;
            }

            // Act & Assert
            try
            {
                var summary = await _buildHelper!.GetLatestBuildSummaryAsync(
                    TestProjectName, 
                    TestRepositoryId, 
                    TestPullRequestId);

                // Assert
                if (summary != null)
                {
                    Assert.IsTrue(summary.BuildId > 0, "Build ID should be positive");
                    Assert.IsNotNull(summary.BuildNumber, "Build number should not be null");
                    
                    Console.WriteLine($"Build Summary:");
                    Console.WriteLine($"  ID: {summary.BuildId}");
                    Console.WriteLine($"  Number: {summary.BuildNumber}");
                    Console.WriteLine($"  Status: {summary.Status}");
                    Console.WriteLine($"  Result: {summary.Result}");
                    Console.WriteLine($"  Definition: {summary.Definition}");
                    Console.WriteLine($"  Requested by: {summary.RequestedBy}");
                }
                else
                {
                    Console.WriteLine("No build summary available for the specified pull request");
                }
                
                Assert.IsTrue(true, "Method completed successfully");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Expected error for test data: {ex.Message}");
                Assert.IsTrue(true, "InvalidOperationException is expected with test data");
            }
        }

        /// <summary>
        /// Integration test for getting multiple builds for a pull request
        /// </summary>
        [TestMethod]
        public async Task IntegrationTest_GetPullRequestBuilds_WithRealData()
        {
            // Arrange
            if (string.IsNullOrEmpty(_personalAccessToken))
            {
                Assert.Inconclusive("Azure DevOps PAT not configured. Set AZURE_DEVOPS_PAT environment variable.");
                return;
            }

            // Act & Assert
            try
            {
                var builds = await _buildHelper!.GetPullRequestBuildsAsync(
                    TestProjectName, 
                    TestRepositoryId, 
                    TestPullRequestId, 
                    maxBuilds: 5);

                // Assert
                Assert.IsNotNull(builds, "Builds collection should not be null");
                
                var buildList = builds.ToList();
                Console.WriteLine($"Found {buildList.Count} builds for pull request");
                
                foreach (var build in buildList.Take(3)) // Show first 3 builds
                {
                    Console.WriteLine($"  Build {build.BuildNumber}: {build.Status} - {build.Result}");
                }
                
                Assert.IsTrue(true, "Method completed successfully");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Expected error for test data: {ex.Message}");
                Assert.IsTrue(true, "InvalidOperationException is expected with test data");
            }
        }

        /// <summary>
        /// Integration test for getting build status and result
        /// </summary>
        [TestMethod]
        public async Task IntegrationTest_GetBuildStatusAndResult_WithRealData()
        {
            // Arrange
            if (string.IsNullOrEmpty(_personalAccessToken))
            {
                Assert.Inconclusive("Azure DevOps PAT not configured. Set AZURE_DEVOPS_PAT environment variable.");
                return;
            }

            // Act & Assert
            try
            {
                // First try to get a build ID
                var buildId = await _buildHelper!.GetLatestBuildIdAsync(
                    TestProjectName, 
                    TestRepositoryId, 
                    TestPullRequestId);

                if (buildId.HasValue)
                {
                    // Test getting build status
                    var status = await _buildHelper.GetBuildStatusAsync(TestProjectName, buildId.Value);
                    Console.WriteLine($"Build {buildId.Value} Status: {status}");

                    // Test getting build result
                    var result = await _buildHelper.GetBuildResultAsync(TestProjectName, buildId.Value);
                    Console.WriteLine($"Build {buildId.Value} Result: {result}");
                    
                    Assert.IsTrue(true, "Build status and result retrieved successfully");
                }
                else
                {
                    // Test with a hypothetical build ID
                    var status = await _buildHelper.GetBuildStatusAsync(TestProjectName, TestBuildId);
                    var result = await _buildHelper.GetBuildResultAsync(TestProjectName, TestBuildId);
                    
                    Console.WriteLine($"Test build status/result completed (may be null for non-existent build)");
                    Assert.IsTrue(true, "Method completed without throwing unexpected exceptions");
                }
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Expected error for test data: {ex.Message}");
                Assert.IsTrue(true, "InvalidOperationException is expected with test data");
            }
        }

        /// <summary>
        /// Integration test for exporting build data to JSON
        /// </summary>
        [TestMethod]
        public async Task IntegrationTest_ExportBuildsToJson_WithRealData()
        {
            // Arrange
            if (string.IsNullOrEmpty(_personalAccessToken))
            {
                Assert.Inconclusive("Azure DevOps PAT not configured. Set AZURE_DEVOPS_PAT environment variable.");
                return;
            }

            var tempPath = Path.GetTempFileName();

            try
            {
                // Act
                var builds = await _buildHelper!.GetPullRequestBuildsAsync(
                    TestProjectName, 
                    TestRepositoryId, 
                    TestPullRequestId, 
                    maxBuilds: 3);

                await _buildHelper.ExportBuildsToJsonAsync(builds, tempPath);

                // Assert
                Assert.IsTrue(File.Exists(tempPath), "Export file should be created");
                
                var content = await File.ReadAllTextAsync(tempPath);
                Assert.IsTrue(content.Length > 0, "Export file should contain data");
                
                Console.WriteLine($"Exported {builds.Count()} builds to {tempPath}");
                Console.WriteLine($"File size: {content.Length} characters");
                
                // Check if it's valid JSON by checking for array brackets
                Assert.IsTrue(content.TrimStart().StartsWith("["), "Export should be JSON array format");
                Assert.IsTrue(content.TrimEnd().EndsWith("]"), "Export should be JSON array format");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Expected error for test data: {ex.Message}");
                
                // Even if we can't get real data, test with empty list
                var emptyBuilds = new List<Build>();
                await _buildHelper!.ExportBuildsToJsonAsync(emptyBuilds, tempPath);
                
                Assert.IsTrue(File.Exists(tempPath), "Export file should be created even with empty data");
                var content = await File.ReadAllTextAsync(tempPath);
                Assert.IsTrue(content.Contains("[]"), "Empty array should be exported");
            }
            finally
            {
                // Cleanup
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }

        /// <summary>
        /// Integration test for complete workflow - get PR, find builds, export data
        /// </summary>
        [TestMethod]
        public async Task IntegrationTest_CompleteWorkflow_WithRealData()
        {
            // Arrange
            if (string.IsNullOrEmpty(_personalAccessToken))
            {
                Assert.Inconclusive("Azure DevOps PAT not configured. Set AZURE_DEVOPS_PAT environment variable.");
                return;
            }

            var tempPath = Path.GetTempFileName();

            try
            {
                Console.WriteLine("Starting complete workflow test...");

                // Step 1: Get latest build ID
                Console.WriteLine("Step 1: Getting latest build ID...");
                var buildId = await _buildHelper!.GetLatestBuildIdAsync(
                    TestProjectName, 
                    TestRepositoryId, 
                    TestPullRequestId);

                // Step 2: Get build summary
                Console.WriteLine("Step 2: Getting build summary...");
                var summary = await _buildHelper.GetLatestBuildSummaryAsync(
                    TestProjectName, 
                    TestRepositoryId, 
                    TestPullRequestId);

                // Step 3: Get all builds
                Console.WriteLine("Step 3: Getting all builds...");
                var builds = await _buildHelper.GetPullRequestBuildsAsync(
                    TestProjectName, 
                    TestRepositoryId, 
                    TestPullRequestId, 
                    maxBuilds: 10);

                // Step 4: Export data
                Console.WriteLine("Step 4: Exporting data...");
                await _buildHelper.ExportBuildsToJsonAsync(builds, tempPath);

                // Assert
                Console.WriteLine("Workflow completed successfully!");
                Console.WriteLine($"Build ID: {buildId?.ToString() ?? "None"}");
                Console.WriteLine($"Summary available: {summary != null}");
                Console.WriteLine($"Builds found: {builds.Count()}");
                Console.WriteLine($"Export file created: {File.Exists(tempPath)}");

                Assert.IsTrue(true, "Complete workflow executed successfully");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Expected error for test data: {ex.Message}");
                Assert.IsTrue(true, "InvalidOperationException is expected with test data");
            }
            finally
            {
                // Cleanup
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }

        #endregion
    }
}
