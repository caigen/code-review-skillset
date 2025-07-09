using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json;
using System.Text;

namespace Common
{
    public class BuildReportHelper : IDisposable
    {
        private readonly string _organizationUrl;
        private readonly string _personalAccessToken;
        private readonly VssConnection _connection;

        public BuildReportHelper(string organizationUrl, string personalAccessToken)
        {
            _organizationUrl = organizationUrl ?? throw new ArgumentNullException(nameof(organizationUrl));
            _personalAccessToken = personalAccessToken ?? throw new ArgumentNullException(nameof(personalAccessToken));
            
            var credentials = new VssBasicCredential(string.Empty, _personalAccessToken);
            _connection = new VssConnection(new Uri(_organizationUrl), credentials);
        }

        /// <summary>
        /// Gets a comprehensive build report for a specific build
        /// </summary>
        /// <param name="projectName">The name of the Azure DevOps project</param>
        /// <param name="buildId">The ID of the build</param>
        /// <returns>A detailed build report</returns>
        public async Task<BuildReport> GetBuildReportAsync(string projectName, int buildId)
        {
            if (string.IsNullOrWhiteSpace(projectName))
                throw new ArgumentException("Project name cannot be null or empty", nameof(projectName));

            if (buildId <= 0)
                throw new ArgumentException("Build ID must be greater than 0", nameof(buildId));

            try
            {
                using var buildClient = _connection.GetClient<BuildHttpClient>();
                
                // Get the build details
                var build = await buildClient.GetBuildAsync(projectName, buildId);
                
                // Get build timeline for task details
                var timeline = await buildClient.GetBuildTimelineAsync(projectName, buildId);
                
                // Get build artifacts
                var artifacts = await buildClient.GetArtifactsAsync(projectName, buildId);
                
                // Get test results if available
                var testResults = await GetTestResultsSummaryAsync(buildClient, projectName, buildId);
                
                return new BuildReport
                {
                    BuildId = build.Id,
                    BuildNumber = build.BuildNumber,
                    ProjectName = projectName,
                    DefinitionName = build.Definition?.Name,
                    DefinitionId = build.Definition?.Id,
                    Status = build.Status?.ToString(),
                    Result = build.Result?.ToString(),
                    QueueTime = build.QueueTime,
                    StartTime = build.StartTime,
                    FinishTime = build.FinishTime,
                    Duration = build.FinishTime - build.StartTime,
                    SourceBranch = build.SourceBranch,
                    SourceVersion = build.SourceVersion,
                    RequestedBy = build.RequestedBy?.DisplayName,
                    RequestedFor = build.RequestedFor?.DisplayName,
                    Reason = build.Reason.ToString(),
                    Repository = build.Repository?.Name,
                    RepositoryType = build.Repository?.Type,
                    //Tasks = ExtractTasksFromTimeline(timeline),
                    /*Artifacts = artifacts?.Select(a => new BuildArtifactInfo
                    {
                        Name = a.Name,
                        Type = a.Resource?.Type,
                        DownloadUrl = a.Resource?.DownloadUrl
                    }).ToList() ?? new List<BuildArtifactInfo>(),
                    */
                    TestResults = testResults,
                    WebUrl = null // Simplified - could be enhanced to parse build links properly
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve build report for build {buildId} in project {projectName}", ex);
            }
        }

        /// <summary>
        /// Gets build reports for multiple builds
        /// </summary>
        /// <param name="projectName">The name of the Azure DevOps project</param>
        /// <param name="buildIds">Collection of build IDs</param>
        /// <returns>Collection of build reports</returns>
        public async Task<IEnumerable<BuildReport>> GetMultipleBuildReportsAsync(string projectName, IEnumerable<int> buildIds)
        {
            if (string.IsNullOrWhiteSpace(projectName))
                throw new ArgumentException("Project name cannot be null or empty", nameof(projectName));

            if (buildIds == null || !buildIds.Any())
                throw new ArgumentException("Build IDs collection cannot be null or empty", nameof(buildIds));

            var reports = new List<BuildReport>();
            
            foreach (var buildId in buildIds)
            {
                try
                {
                    var report = await GetBuildReportAsync(projectName, buildId);
                    reports.Add(report);
                }
                catch (Exception ex)
                {
                    // Log the error but continue with other builds
                    Console.WriteLine($"Failed to get report for build {buildId}: {ex.Message}");
                }
            }
            
            return reports;
        }

        /// <summary>
        /// Gets build reports for recent builds of a specific pipeline definition
        /// </summary>
        /// <param name="projectName">The name of the Azure DevOps project</param>
        /// <param name="pipelineDefinitionId">The ID of the pipeline definition</param>
        /// <param name="count">Number of recent builds to retrieve (default: 10)</param>
        /// <param name="status">Optional status filter</param>
        /// <returns>Collection of build reports</returns>
        public async Task<IEnumerable<BuildReport>> GetRecentBuildReportsAsync(
            string projectName, 
            int pipelineDefinitionId, 
            int count = 10,
            BuildStatus? status = null)
        {
            if (string.IsNullOrWhiteSpace(projectName))
                throw new ArgumentException("Project name cannot be null or empty", nameof(projectName));

            if (pipelineDefinitionId <= 0)
                throw new ArgumentException("Pipeline definition ID must be greater than 0", nameof(pipelineDefinitionId));

            if (count <= 0)
                throw new ArgumentException("Count must be greater than 0", nameof(count));

            try
            {
                using var buildClient = _connection.GetClient<BuildHttpClient>();
                
                // Get recent builds for the pipeline definition
                var builds = await buildClient.GetBuildsAsync(
                    project: projectName,
                    definitions: new[] { pipelineDefinitionId },
                    top: count,
                    queryOrder: BuildQueryOrder.FinishTimeDescending,
                    statusFilter: status
                );

                var buildIds = builds.Select(b => b.Id);
                return await GetMultipleBuildReportsAsync(projectName, buildIds);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve recent build reports for definition {pipelineDefinitionId} in project {projectName}", ex);
            }
        }

        /// <summary>
        /// Gets a summary report for all builds within a date range
        /// </summary>
        /// <param name="projectName">The name of the Azure DevOps project</param>
        /// <param name="fromDate">Start date for the report</param>
        /// <param name="toDate">End date for the report</param>
        /// <param name="pipelineDefinitionId">Optional pipeline definition ID filter</param>
        /// <returns>Build summary report</returns>
        public async Task<BuildSummaryReport> GetBuildSummaryReportAsync(
            string projectName, 
            DateTime fromDate, 
            DateTime toDate,
            int? pipelineDefinitionId = null)
        {
            if (string.IsNullOrWhiteSpace(projectName))
                throw new ArgumentException("Project name cannot be null or empty", nameof(projectName));

            if (fromDate >= toDate)
                throw new ArgumentException("From date must be before to date");

            try
            {
                using var buildClient = _connection.GetClient<BuildHttpClient>();
                
                var builds = await buildClient.GetBuildsAsync(
                    project: projectName,
                    definitions: pipelineDefinitionId.HasValue ? new[] { pipelineDefinitionId.Value } : null,
                    minFinishTime: fromDate,
                    maxFinishTime: toDate,
                    top: 1000 // Adjust as needed
                );

                var successfulBuilds = builds.Where(b => b.Result == BuildResult.Succeeded).Count();
                var failedBuilds = builds.Where(b => b.Result == BuildResult.Failed).Count();
                var canceledBuilds = builds.Where(b => b.Result == BuildResult.Canceled).Count();
                var partiallySucceededBuilds = builds.Where(b => b.Result == BuildResult.PartiallySucceeded).Count();
                
                var completedBuilds = builds.Where(b => b.StartTime.HasValue && b.FinishTime.HasValue).ToList();
                var averageDuration = completedBuilds.Any() 
                    ? TimeSpan.FromTicks((long)completedBuilds.Average(b => (b.FinishTime!.Value - b.StartTime!.Value).Ticks))
                    : TimeSpan.Zero;

                return new BuildSummaryReport
                {
                    ProjectName = projectName,
                    PipelineDefinitionId = pipelineDefinitionId,
                    FromDate = fromDate,
                    ToDate = toDate,
                    TotalBuilds = builds.Count(),
                    SuccessfulBuilds = successfulBuilds,
                    FailedBuilds = failedBuilds,
                    CanceledBuilds = canceledBuilds,
                    PartiallySucceededBuilds = partiallySucceededBuilds,
                    SuccessRate = builds.Any() ? (double)successfulBuilds / builds.Count() * 100 : 0,
                    AverageBuildDuration = averageDuration,
                    UniqueDefinitions = builds.Where(b => b.Definition != null).Select(b => b.Definition!.Name).Distinct().Count(),
                    GeneratedOn = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve build summary report for project {projectName}", ex);
            }
        }

        /// <summary>
        /// Exports a build report to JSON format
        /// </summary>
        /// <param name="report">The build report to export</param>
        /// <param name="filePath">Path to save the JSON file</param>
        public async Task ExportBuildReportToJsonAsync(BuildReport report, string filePath)
        {
            if (report == null)
                throw new ArgumentNullException(nameof(report));

            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonConvert.SerializeObject(report, Formatting.Indented);
            await File.WriteAllTextAsync(filePath, json, Encoding.UTF8);
        }

        /// <summary>
        /// Exports multiple build reports to JSON format
        /// </summary>
        /// <param name="reports">The build reports to export</param>
        /// <param name="filePath">Path to save the JSON file</param>
        public async Task ExportBuildReportsToJsonAsync(IEnumerable<BuildReport> reports, string filePath)
        {
            if (reports == null)
                throw new ArgumentNullException(nameof(reports));

            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonConvert.SerializeObject(reports, Formatting.Indented);
            await File.WriteAllTextAsync(filePath, json, Encoding.UTF8);
        }

        private List<BuildTaskInfo> ExtractTasksFromTimeline(Timeline? timeline)
        {
            var tasks = new List<BuildTaskInfo>();
            
            if (timeline?.Records == null)
                return tasks;

            foreach (var record in timeline.Records.Where(r => r.RecordType == "Task"))
            {
                tasks.Add(new BuildTaskInfo
                {
                    Id = record.Id,
                    Name = record.Name,
                    State = record.State?.ToString(),
                    Result = record.Result?.ToString(),
                    StartTime = record.StartTime,
                    FinishTime = record.FinishTime,
                    Duration = record.FinishTime - record.StartTime,
                    PercentComplete = record.PercentComplete,
                    Order = record.Order,
                    ErrorCount = record.ErrorCount,
                    WarningCount = record.WarningCount
                });
            }
            
            return tasks.OrderBy(t => t.Order).ToList();
        }

        private async Task<BuildTestResultsSummary?> GetTestResultsSummaryAsync(BuildHttpClient buildClient, string projectName, int buildId)
        {
            try
            {
                // Get test management client
                using var testClient = _connection.GetClient<TestManagementHttpClient>();
                
                // Get test runs associated with the build
                var testRuns = await testClient.GetTestRunsAsync(
                    project: projectName,
                    buildUri: $"vstfs:///Build/Build/{buildId}");

                if (!testRuns.Any())
                {
                    return new BuildTestResultsSummary
                    {
                        TotalTests = 0,
                        PassedTests = 0,
                        FailedTests = 0,
                        SkippedTests = 0
                    };
                }

                int totalTests = 0;
                int passedTests = 0;
                int failedTests = 0;
                int skippedTests = 0;

                // Aggregate results from all test runs for this build
                foreach (var testRun in testRuns)
                {
                    // Get test results for each test run
                    var testResults = await testClient.GetTestResultsAsync(
                        project: projectName,
                        runId: testRun.Id);

                    foreach (var result in testResults)
                    {
                        totalTests++;
                            
                        switch (result.Outcome?.ToLowerInvariant())
                        {
                            case "passed":
                                passedTests++;
                                break;
                            case "failed":
                                failedTests++;
                                break;
                            case "skipped":
                            case "notexecuted":
                            case "inconclusive":
                                skippedTests++;
                                break;
                            default:
                                // Handle other outcomes (e.g., "aborted", "timeout") as failed
                                failedTests++;
                                break;
                        }
                    }
                }

                return new BuildTestResultsSummary
                {
                    TotalTests = totalTests,
                    PassedTests = passedTests,
                    FailedTests = failedTests,
                    SkippedTests = skippedTests
                };
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the entire build report
                Console.WriteLine($"Warning: Failed to retrieve test results for build {buildId}: {ex.Message}");
                return new BuildTestResultsSummary
                {
                    TotalTests = 0,
                    PassedTests = 0,
                    FailedTests = 0,
                    SkippedTests = 0
                };
            }
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }

    /// <summary>
    /// Represents a comprehensive build report
    /// </summary>
    public class BuildReport
    {
        public int BuildId { get; set; }
        public string? BuildNumber { get; set; }
        public string? ProjectName { get; set; }
        public string? DefinitionName { get; set; }
        public int? DefinitionId { get; set; }
        public string? Status { get; set; }
        public string? Result { get; set; }
        public DateTime? QueueTime { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? FinishTime { get; set; }
        public TimeSpan? Duration { get; set; }
        public string? SourceBranch { get; set; }
        public string? SourceVersion { get; set; }
        public string? RequestedBy { get; set; }
        public string? RequestedFor { get; set; }
        public string? Reason { get; set; }
        public string? Repository { get; set; }
        public string? RepositoryType { get; set; }

        // [gencai] shorten data.
        //public List<BuildTaskInfo> Tasks { get; set; } = new List<BuildTaskInfo>();
        // public List<BuildArtifactInfo> Artifacts { get; set; } = new List<BuildArtifactInfo>();
        public BuildTestResultsSummary? TestResults { get; set; }
        public string? WebUrl { get; set; }
    }

    /// <summary>
    /// Represents information about a build task
    /// </summary>
    public class BuildTaskInfo
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public string? State { get; set; }
        public string? Result { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? FinishTime { get; set; }
        public TimeSpan? Duration { get; set; }
        public int? PercentComplete { get; set; }
        public int? Order { get; set; }
        public int? ErrorCount { get; set; }
        public int? WarningCount { get; set; }
    }

    /// <summary>
    /// Represents information about a build artifact
    /// </summary>
    public class BuildArtifactInfo
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? DownloadUrl { get; set; }
    }

    /// <summary>
    /// Represents test results summary for a build
    /// </summary>
    public class BuildTestResultsSummary
    {
        public int TotalTests { get; set; }
        public int PassedTests { get; set; }
        public int FailedTests { get; set; }
        public int SkippedTests { get; set; }
        public double PassRate => TotalTests > 0 ? (double)PassedTests / TotalTests * 100 : 0;
    }

    /// <summary>
    /// Represents a summary report for multiple builds
    /// </summary>
    public class BuildSummaryReport
    {
        public string? ProjectName { get; set; }
        public int? PipelineDefinitionId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalBuilds { get; set; }
        public int SuccessfulBuilds { get; set; }
        public int FailedBuilds { get; set; }
        public int CanceledBuilds { get; set; }
        public int PartiallySucceededBuilds { get; set; }
        public double SuccessRate { get; set; }
        public TimeSpan AverageBuildDuration { get; set; }
        public int UniqueDefinitions { get; set; }
        public DateTime GeneratedOn { get; set; }
    }
}
