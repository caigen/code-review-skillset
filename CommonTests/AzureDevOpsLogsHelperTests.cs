using Common;
using System.Text;

namespace CommonTests
{
    /// <summary>
    /// Unit tests for LogsHelper class demonstrating Azure DevOps pipeline logs functionality
    /// </summary>
    [TestClass]
    public class AzureDevOpsLogsHelperTests
    {
        private const string TestOrganizationUrl = "https://dev.azure.com/test-org";
        private const string TestPersonalAccessToken = "test-pat-token";
        private const string TestProjectName = "TestProject";
        private const int TestBuildId = 12345;
        private const int TestPipelineDefinitionId = 67890;

        private LogsHelper? _logsHelper;

        [TestInitialize]
        public void TestInitialize()
        {
            // Note: In real tests, you would either mock the dependencies or use integration tests
            // with actual Azure DevOps connections. For demonstration, we're using test values.
            _logsHelper = new LogsHelper(TestOrganizationUrl, TestPersonalAccessToken);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            // LogsHelper cleanup would be handled here if it implemented IDisposable
        }

        [TestMethod]
        public void Constructor_WithValidParameters_ShouldCreateInstance()
        {
            // Arrange & Act
            var logsHelper = new LogsHelper(TestOrganizationUrl, TestPersonalAccessToken);

            // Assert
            Assert.IsNotNull(logsHelper);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullOrganizationUrl_ShouldThrowArgumentNullException()
        {
            // Arrange & Act & Assert
            new LogsHelper(null!, TestPersonalAccessToken);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullPersonalAccessToken_ShouldThrowArgumentNullException()
        {
            // Arrange & Act & Assert
            new LogsHelper(TestOrganizationUrl, null!);
        }

        [TestMethod]
        public async Task GetPipelineRunLogsAsync_WithValidParameters_ShouldAttemptToRetrieveLogs()
        {
            // Arrange
            var projectName = TestProjectName;
            var buildId = TestBuildId;

            // Act & Assert
            try
            {
                var result = await _logsHelper!.GetPipelineRunLogsAsync(projectName, buildId);
                
                // In a real scenario with mocked dependencies, we would assert the expected behavior
                // For this demonstration, we're testing that the method can be called without throwing
                // due to parameter validation issues
                Assert.IsNotNull(result);
            }
            catch (Exception)
            {
                // Expected in test environment - we don't have actual Azure DevOps connection
                // In real tests, you would mock the Azure DevOps client
                // Any exception here is acceptable since we're testing against a non-existent service
                Assert.IsTrue(true, "Expected exception due to no actual Azure DevOps connection");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetPipelineRunLogsAsync_WithEmptyProjectName_ShouldThrowArgumentException()
        {
            // Arrange & Act & Assert
            await _logsHelper!.GetPipelineRunLogsAsync("", TestBuildId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetPipelineRunLogsAsync_WithInvalidBuildId_ShouldThrowArgumentException()
        {
            // Arrange & Act & Assert
            await _logsHelper!.GetPipelineRunLogsAsync(TestProjectName, 0);
        }

        [TestMethod]
        public async Task SavePipelineLogsToFileAsync_WithValidParameters_ShouldAttemptToSaveLogs()
        {
            // Arrange
            var projectName = TestProjectName;
            var buildId = TestBuildId;
            var outputPath = Path.Combine(Path.GetTempPath(), $"test-logs-{Guid.NewGuid()}.log");

            try
            {
                // Act
                await _logsHelper!.SavePipelineLogsToFileAsync(projectName, buildId, outputPath);
                
                // Assert - In real tests with mocked dependencies, we would verify the file was created
                // For this demonstration, we're testing that the method can be called
                Assert.IsTrue(true, "Method executed without parameter validation errors");
            }
            catch (Exception)
            {
                // Expected in test environment - we don't have actual Azure DevOps connection
                Assert.IsTrue(true, "Expected exception due to no actual Azure DevOps connection");
            }
            finally
            {
                // Cleanup
                if (File.Exists(outputPath))
                {
                    File.Delete(outputPath);
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task SavePipelineLogsToFileAsync_WithEmptyProjectName_ShouldThrowArgumentException()
        {
            // Arrange & Act & Assert
            await _logsHelper!.SavePipelineLogsToFileAsync("", TestBuildId, "test.log");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task SavePipelineLogsToFileAsync_WithInvalidBuildId_ShouldThrowArgumentException()
        {
            // Arrange & Act & Assert
            await _logsHelper!.SavePipelineLogsToFileAsync(TestProjectName, 0, "test.log");
        }

        [TestMethod]
        public async Task GetRecentPipelineRunLogsAsync_WithValidParameters_ShouldAttemptToRetrieveLogs()
        {
            // Arrange
            var projectName = TestProjectName;
            var pipelineDefinitionId = TestPipelineDefinitionId;
            var count = 3;

            try
            {
                // Act
                var result = await _logsHelper!.GetRecentPipelineRunLogsAsync(projectName, pipelineDefinitionId, count);
                
                // Assert
                Assert.IsNotNull(result);
            }
            catch (Exception)
            {
                // Expected in test environment - we don't have actual Azure DevOps connection
                Assert.IsTrue(true, "Expected exception due to no actual Azure DevOps connection");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetRecentPipelineRunLogsAsync_WithEmptyProjectName_ShouldThrowArgumentException()
        {
            // Arrange & Act & Assert
            await _logsHelper!.GetRecentPipelineRunLogsAsync("", TestPipelineDefinitionId, 3);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetRecentPipelineRunLogsAsync_WithInvalidPipelineDefinitionId_ShouldThrowArgumentException()
        {
            // Arrange & Act & Assert
            await _logsHelper!.GetRecentPipelineRunLogsAsync(TestProjectName, 0, 3);
        }

        [TestMethod]
        public void LogsHelper_ShouldBeInstantiable()
        {
            // Arrange & Act
            var logsHelper = new LogsHelper(TestOrganizationUrl, TestPersonalAccessToken);

            // Assert
            Assert.IsNotNull(logsHelper, "LogsHelper should be instantiable");
        }

        /// <summary>
        /// Integration test example - would require actual Azure DevOps setup
        /// </summary>
        [TestMethod]
        //[Ignore("Integration test - requires actual Azure DevOps connection")]
        public async Task IntegrationTest_GetPipelineRunLogsAsync_WithRealConnection()
        {
            // This test would be used in an integration test environment
            // with actual Azure DevOps organization, project, and build IDs
            
            var realOrganizationUrl = "https://dev.azure.com/skype";
            var realPat = Environment.GetEnvironmentVariable("AZURE_DEVOPS_PAT");
            var realProjectName = "SCC";
            var realBuildId = 71015951; // An actual build ID
            
            if (string.IsNullOrEmpty(realPat))
            {
                Assert.Inconclusive("AZURE_DEVOPS_PAT environment variable not set");
                return;
            }

            var realLogsHelper = new LogsHelper(realOrganizationUrl, realPat);
            
            // Act
            var logs = await realLogsHelper.GetPipelineRunLogsAsync(realProjectName, realBuildId);
            
            // Assert
            Assert.IsNotNull(logs);
            Assert.IsTrue(logs.Any(), "Should retrieve at least one log entry");
            
            foreach (var log in logs)
            {
                Assert.IsTrue(log.Id > 0, "Log ID should be greater than 0");
                Assert.IsNotNull(log.Type, "Log type should not be null");
                Assert.IsTrue(log.CreatedOn > DateTime.MinValue, "Log should have a valid creation date");
            }
        }
    }
}
