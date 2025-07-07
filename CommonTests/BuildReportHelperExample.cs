using Common;

namespace CommonTests
{
    /// <summary>
    /// Example usage of BuildReportHelper
    /// </summary>
    public class BuildReportHelperExample
    {
        private readonly BuildReportHelper _buildReportHelper;

        public BuildReportHelperExample()
        {
            // Initialize with your Azure DevOps organization URL and Personal Access Token
            var organizationUrl = "https://dev.azure.com/yourorganization";
            var personalAccessToken = "your-pat-token";
            
            _buildReportHelper = new BuildReportHelper(organizationUrl, personalAccessToken);
        }

        /// <summary>
        /// Example: Get a detailed report for a specific build
        /// </summary>
        public async Task<BuildReport> GetSingleBuildReportExample()
        {
            var projectName = "YourProject";
            var buildId = 12345;
            
            var report = await _buildReportHelper.GetBuildReportAsync(projectName, buildId);
            
            Console.WriteLine($"Build {report.BuildNumber} - Status: {report.Status}, Result: {report.Result}");
            Console.WriteLine($"Duration: {report.Duration}");
            Console.WriteLine($"Tasks: {report.Tasks.Count}");
            Console.WriteLine($"Artifacts: {report.Artifacts.Count}");
            
            return report;
        }

        /// <summary>
        /// Example: Get reports for recent builds of a pipeline
        /// </summary>
        public async Task<IEnumerable<BuildReport>> GetRecentBuildsExample()
        {
            var projectName = "YourProject";
            var pipelineDefinitionId = 123;
            var count = 5;
            
            var reports = await _buildReportHelper.GetRecentBuildReportsAsync(
                projectName, 
                pipelineDefinitionId, 
                count);
            
            foreach (var report in reports)
            {
                Console.WriteLine($"Build {report.BuildNumber}: {report.Result} ({report.Duration})");
            }
            
            return reports;
        }

        /// <summary>
        /// Example: Get a summary report for builds in a date range
        /// </summary>
        public async Task<BuildSummaryReport> GetBuildSummaryExample()
        {
            var projectName = "YourProject";
            var fromDate = DateTime.Now.AddDays(-30); // Last 30 days
            var toDate = DateTime.Now;
            
            var summary = await _buildReportHelper.GetBuildSummaryReportAsync(
                projectName, 
                fromDate, 
                toDate);
            
            Console.WriteLine($"Build Summary for {projectName}:");
            Console.WriteLine($"Total Builds: {summary.TotalBuilds}");
            Console.WriteLine($"Success Rate: {summary.SuccessRate:F2}%");
            Console.WriteLine($"Average Duration: {summary.AverageBuildDuration}");
            Console.WriteLine($"Failed Builds: {summary.FailedBuilds}");
            
            return summary;
        }

        /// <summary>
        /// Example: Export build reports to JSON files
        /// </summary>
        public async Task ExportReportsExample()
        {
            var projectName = "YourProject";
            var buildId = 12345;
            
            // Get a single build report
            var report = await _buildReportHelper.GetBuildReportAsync(projectName, buildId);
            
            // Export to JSON
            await _buildReportHelper.ExportBuildReportToJsonAsync(
                report, 
                $@"c:\temp\build-report-{buildId}.json");
            
            Console.WriteLine($"Build report exported to JSON file");
        }

        /// <summary>
        /// Example: Get multiple build reports and export them
        /// </summary>
        public async Task ExportMultipleReportsExample()
        {
            var projectName = "YourProject";
            var buildIds = new[] { 12345, 12346, 12347 };
            
            // Get multiple build reports
            var reports = await _buildReportHelper.GetMultipleBuildReportsAsync(projectName, buildIds);
            
            // Export to JSON
            await _buildReportHelper.ExportBuildReportsToJsonAsync(
                reports, 
                @"c:\temp\multiple-build-reports.json");
            
            Console.WriteLine($"Multiple build reports exported to JSON file");
        }

        public void Dispose()
        {
            _buildReportHelper?.Dispose();
        }
    }
}
