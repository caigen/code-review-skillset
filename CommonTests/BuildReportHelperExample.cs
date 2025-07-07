using Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
        private const string TestProjectName = "YourProject";
        private const int TestBuildId = 12345;
        private const int TestPipelineDefinitionId = 123;

        [TestInitialize]
        public void TestInitialize()
        {
            // Initialize with test Azure DevOps organization URL and Personal Access Token
            _buildReportHelper = new BuildReportHelper(TestOrganizationUrl, TestPersonalAccessToken);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _buildReportHelper?.Dispose();
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task GetBuildReportAsync_WithValidBuildId_ShouldReturnBuildReport()
        {
            // Arrange
            var projectName = TestProjectName;
            var buildId = TestBuildId;
            
            // Act
            var report = await _buildReportHelper!.GetBuildReportAsync(projectName, buildId);
            
            // Assert
            Assert.IsNotNull(report);
            Assert.AreEqual(buildId, report.BuildId);
            Assert.AreEqual(projectName, report.ProjectName);
            Assert.IsNotNull(report.BuildNumber);
            
            // Log results for verification
            Console.WriteLine($"Build {report.BuildNumber} - Status: {report.Status}, Result: {report.Result}");
            Console.WriteLine($"Duration: {report.Duration}");
            Console.WriteLine($"Tasks: {report.Tasks.Count}");
            Console.WriteLine($"Artifacts: {report.Artifacts.Count}");
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task GetBuildReportAsync_WithInvalidBuildId_ShouldThrowException()
        {
            // Arrange
            var projectName = TestProjectName;
            var invalidBuildId = -1;
            
            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                () => _buildReportHelper!.GetBuildReportAsync(projectName, invalidBuildId));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task GetBuildReportAsync_WithNullProjectName_ShouldThrowException()
        {
            // Arrange
            string? nullProjectName = null;
            var buildId = TestBuildId;
            
            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                () => _buildReportHelper!.GetBuildReportAsync(nullProjectName!, buildId));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task GetRecentBuildReportsAsync_WithValidParameters_ShouldReturnBuildReports()
        {
            // Arrange
            var projectName = TestProjectName;
            var pipelineDefinitionId = TestPipelineDefinitionId;
            var count = 5;
            
            // Act
            var reports = await _buildReportHelper!.GetRecentBuildReportsAsync(
                projectName, 
                pipelineDefinitionId, 
                count);
            
            // Assert
            Assert.IsNotNull(reports);
            Assert.IsTrue(reports.Count() <= count);
            
            foreach (var report in reports)
            {
                Assert.IsNotNull(report);
                Assert.AreEqual(projectName, report.ProjectName);
                Console.WriteLine($"Build {report.BuildNumber}: {report.Result} ({report.Duration})");
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task GetRecentBuildReportsAsync_WithInvalidDefinitionId_ShouldThrowException()
        {
            // Arrange
            var projectName = TestProjectName;
            var invalidPipelineDefinitionId = -1;
            var count = 5;
            
            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                () => _buildReportHelper!.GetRecentBuildReportsAsync(projectName, invalidPipelineDefinitionId, count));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task GetBuildSummaryReportAsync_WithValidDateRange_ShouldReturnSummaryReport()
        {
            // Arrange
            var projectName = TestProjectName;
            var fromDate = DateTime.Now.AddDays(-30); // Last 30 days
            var toDate = DateTime.Now;
            
            // Act
            var summary = await _buildReportHelper!.GetBuildSummaryReportAsync(
                projectName, 
                fromDate, 
                toDate);
            
            // Assert
            Assert.IsNotNull(summary);
            Assert.AreEqual(projectName, summary.ProjectName);
            Assert.AreEqual(fromDate.Date, summary.FromDate.Date);
            Assert.AreEqual(toDate.Date, summary.ToDate.Date);
            Assert.IsTrue(summary.TotalBuilds >= 0);
            Assert.IsTrue(summary.SuccessRate >= 0 && summary.SuccessRate <= 100);
            
            // Log results for verification
            Console.WriteLine($"Build Summary for {projectName}:");
            Console.WriteLine($"Total Builds: {summary.TotalBuilds}");
            Console.WriteLine($"Success Rate: {summary.SuccessRate:F2}%");
            Console.WriteLine($"Average Duration: {summary.AverageBuildDuration}");
            Console.WriteLine($"Failed Builds: {summary.FailedBuilds}");
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task GetBuildSummaryReportAsync_WithInvalidDateRange_ShouldThrowException()
        {
            // Arrange
            var projectName = TestProjectName;
            var fromDate = DateTime.Now;
            var toDate = DateTime.Now.AddDays(-1); // Invalid: to date before from date
            
            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                () => _buildReportHelper!.GetBuildSummaryReportAsync(projectName, fromDate, toDate));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task ExportBuildReportToJsonAsync_WithValidReport_ShouldCreateJsonFile()
        {
            // Arrange
            var projectName = TestProjectName;
            var buildId = TestBuildId;
            var tempFilePath = Path.Combine(Path.GetTempPath(), $"build-report-{buildId}-{Guid.NewGuid()}.json");
            
            try
            {
                // Get a build report
                var report = await _buildReportHelper!.GetBuildReportAsync(projectName, buildId);
                
                // Act
                await _buildReportHelper.ExportBuildReportToJsonAsync(report, tempFilePath);
                
                // Assert
                Assert.IsTrue(File.Exists(tempFilePath));
                var fileContent = await File.ReadAllTextAsync(tempFilePath);
                Assert.IsFalse(string.IsNullOrWhiteSpace(fileContent));
                Assert.IsTrue(fileContent.Contains(buildId.ToString()));
                
                Console.WriteLine($"Build report exported to: {tempFilePath}");
            }
            finally
            {
                // Cleanup
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task ExportBuildReportToJsonAsync_WithNullReport_ShouldThrowException()
        {
            // Arrange
            BuildReport? nullReport = null;
            var filePath = Path.Combine(Path.GetTempPath(), "test.json");
            
            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(
                () => _buildReportHelper!.ExportBuildReportToJsonAsync(nullReport!, filePath));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task GetMultipleBuildReportsAsync_WithValidBuildIds_ShouldReturnMultipleReports()
        {
            // Arrange
            var projectName = TestProjectName;
            var buildIds = new[] { TestBuildId, TestBuildId + 1, TestBuildId + 2 };
            var tempFilePath = Path.Combine(Path.GetTempPath(), $"multiple-build-reports-{Guid.NewGuid()}.json");
            
            try
            {
                // Act
                var reports = await _buildReportHelper!.GetMultipleBuildReportsAsync(projectName, buildIds);
                
                // Assert
                Assert.IsNotNull(reports);
                Assert.IsTrue(reports.Any());
                
                // Test export functionality
                await _buildReportHelper.ExportBuildReportsToJsonAsync(reports, tempFilePath);
                Assert.IsTrue(File.Exists(tempFilePath));
                
                var fileContent = await File.ReadAllTextAsync(tempFilePath);
                Assert.IsFalse(string.IsNullOrWhiteSpace(fileContent));
                
                Console.WriteLine($"Retrieved {reports.Count()} build reports and exported to: {tempFilePath}");
            }
            finally
            {
                // Cleanup
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task GetMultipleBuildReportsAsync_WithEmptyBuildIds_ShouldThrowException()
        {
            // Arrange
            var projectName = TestProjectName;
            var emptyBuildIds = Array.Empty<int>();
            
            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                () => _buildReportHelper!.GetMultipleBuildReportsAsync(projectName, emptyBuildIds));
        }

        [TestMethod]
        public void Constructor_WithNullOrganizationUrl_ShouldThrowException()
        {
            // Arrange
            string? nullUrl = null;
            var pat = TestPersonalAccessToken;
            
            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(
                () => new BuildReportHelper(nullUrl!, pat));
        }

        [TestMethod]
        public void Constructor_WithNullPersonalAccessToken_ShouldThrowException()
        {
            // Arrange
            var url = TestOrganizationUrl;
            string? nullPat = null;
            
            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(
                () => new BuildReportHelper(url, nullPat!));
        }

        [TestMethod]
        public void Constructor_WithValidParameters_ShouldCreateInstance()
        {
            // Arrange & Act
            using var helper = new BuildReportHelper(TestOrganizationUrl, TestPersonalAccessToken);
            
            // Assert
            Assert.IsNotNull(helper);
        }
    }
}
