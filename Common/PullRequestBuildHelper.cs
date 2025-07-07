using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json;
using System.Text;

namespace Common
{
    public class PullRequestBuildHelper : IDisposable
    {
        private readonly string _organizationUrl;
        private readonly string _personalAccessToken;
        private readonly VssConnection _connection;

        public PullRequestBuildHelper(string organizationUrl, string personalAccessToken)
        {
            _organizationUrl = organizationUrl ?? throw new ArgumentNullException(nameof(organizationUrl));
            _personalAccessToken = personalAccessToken ?? throw new ArgumentNullException(nameof(personalAccessToken));
            
            var credentials = new VssBasicCredential(string.Empty, _personalAccessToken);
            _connection = new VssConnection(new Uri(_organizationUrl), credentials);
        }

        /// <summary>
        /// Gets the latest build ID for a pull request
        /// </summary>
        /// <param name="projectName">The name of the Azure DevOps project</param>
        /// <param name="repositoryId">The ID or name of the repository</param>
        /// <param name="pullRequestId">The ID of the pull request</param>
        /// <returns>The latest build ID, or null if no builds found</returns>
        public async Task<int?> GetLatestBuildIdAsync(
            string projectName,
            string repositoryId,
            int pullRequestId)
        {
            if (string.IsNullOrWhiteSpace(projectName))
                throw new ArgumentException("Project name cannot be null or empty", nameof(projectName));

            if (string.IsNullOrWhiteSpace(repositoryId))
                throw new ArgumentException("Repository ID cannot be null or empty", nameof(repositoryId));

            if (pullRequestId <= 0)
                throw new ArgumentException("Pull request ID must be greater than 0", nameof(pullRequestId));

            try
            {
                using var gitClient = _connection.GetClient<GitHttpClient>();
                using var buildClient = _connection.GetClient<BuildHttpClient>();

                // Get the pull request details to get the source branch
                var pullRequest = await gitClient.GetPullRequestAsync(
                    projectName,
                    repositoryId,
                    pullRequestId);

                if (pullRequest == null)
                    throw new InvalidOperationException($"Pull request {pullRequestId} not found");

                // Get builds for the source branch of the pull request
                var builds = await buildClient.GetBuildsAsync(
                    projectName,
                    repositoryId: repositoryId,
                    branchName: pullRequest.SourceRefName,
                    top: 1,
                    queryOrder: BuildQueryOrder.FinishTimeDescending);

                return builds?.FirstOrDefault()?.Id;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to get latest build ID for pull request {pullRequestId} in repository {repositoryId}", ex);
            }
        }

        /// <summary>
        /// Gets the latest build for a pull request with detailed information
        /// </summary>
        /// <param name="projectName">The name of the Azure DevOps project</param>
        /// <param name="repositoryId">The ID or name of the repository</param>
        /// <param name="pullRequestId">The ID of the pull request</param>
        /// <returns>The latest build details, or null if no builds found</returns>
        public async Task<Build?> GetLatestBuildAsync(
            string projectName,
            string repositoryId,
            int pullRequestId)
        {
            if (string.IsNullOrWhiteSpace(projectName))
                throw new ArgumentException("Project name cannot be null or empty", nameof(projectName));

            if (string.IsNullOrWhiteSpace(repositoryId))
                throw new ArgumentException("Repository ID cannot be null or empty", nameof(repositoryId));

            if (pullRequestId <= 0)
                throw new ArgumentException("Pull request ID must be greater than 0", nameof(pullRequestId));

            try
            {
                using var gitClient = _connection.GetClient<GitHttpClient>();
                using var buildClient = _connection.GetClient<BuildHttpClient>();

                // Get the pull request details to get the source branch
                var pullRequest = await gitClient.GetPullRequestAsync(
                    projectName,
                    repositoryId,
                    pullRequestId);

                if (pullRequest == null)
                    throw new InvalidOperationException($"Pull request {pullRequestId} not found");

                // Get builds for the source branch of the pull request
                var builds = await buildClient.GetBuildsAsync(
                    projectName,
                    repositoryId: repositoryId,
                    branchName: pullRequest.SourceRefName,
                    top: 1,
                    queryOrder: BuildQueryOrder.FinishTimeDescending);

                return builds?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to get latest build for pull request {pullRequestId} in repository {repositoryId}", ex);
            }
        }

        /// <summary>
        /// Gets all builds associated with a pull request
        /// </summary>
        /// <param name="projectName">The name of the Azure DevOps project</param>
        /// <param name="repositoryId">The ID or name of the repository</param>
        /// <param name="pullRequestId">The ID of the pull request</param>
        /// <param name="maxBuilds">Maximum number of builds to retrieve (default: 10)</param>
        /// <returns>Collection of builds for the pull request</returns>
        public async Task<IEnumerable<Build>> GetPullRequestBuildsAsync(
            string projectName,
            string repositoryId,
            int pullRequestId,
            int maxBuilds = 10)
        {
            if (string.IsNullOrWhiteSpace(projectName))
                throw new ArgumentException("Project name cannot be null or empty", nameof(projectName));

            if (string.IsNullOrWhiteSpace(repositoryId))
                throw new ArgumentException("Repository ID cannot be null or empty", nameof(repositoryId));

            if (pullRequestId <= 0)
                throw new ArgumentException("Pull request ID must be greater than 0", nameof(pullRequestId));

            if (maxBuilds <= 0)
                throw new ArgumentException("Max builds must be greater than 0", nameof(maxBuilds));

            try
            {
                using var gitClient = _connection.GetClient<GitHttpClient>();
                using var buildClient = _connection.GetClient<BuildHttpClient>();

                // Get the pull request details to get the source branch
                var pullRequest = await gitClient.GetPullRequestAsync(
                    projectName,
                    repositoryId,
                    pullRequestId);

                if (pullRequest == null)
                    throw new InvalidOperationException($"Pull request {pullRequestId} not found");

                // Get builds for the source branch of the pull request
                var builds = await buildClient.GetBuildsAsync(
                    projectName,
                    repositoryId: repositoryId,
                    branchName: pullRequest.SourceRefName,
                    top: maxBuilds,
                    queryOrder: BuildQueryOrder.FinishTimeDescending);

                return builds ?? new List<Build>();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to get builds for pull request {pullRequestId} in repository {repositoryId}", ex);
            }
        }

        /// <summary>
        /// Gets build status for a specific build ID
        /// </summary>
        /// <param name="projectName">The name of the Azure DevOps project</param>
        /// <param name="buildId">The ID of the build</param>
        /// <returns>Build status information</returns>
        public async Task<BuildStatus?> GetBuildStatusAsync(string projectName, int buildId)
        {
            if (string.IsNullOrWhiteSpace(projectName))
                throw new ArgumentException("Project name cannot be null or empty", nameof(projectName));

            if (buildId <= 0)
                throw new ArgumentException("Build ID must be greater than 0", nameof(buildId));

            try
            {
                using var buildClient = _connection.GetClient<BuildHttpClient>();

                var build = await buildClient.GetBuildAsync(projectName, buildId);
                return build?.Status;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to get build status for build {buildId} in project {projectName}", ex);
            }
        }

        /// <summary>
        /// Gets build result for a specific build ID
        /// </summary>
        /// <param name="projectName">The name of the Azure DevOps project</param>
        /// <param name="buildId">The ID of the build</param>
        /// <returns>Build result information</returns>
        public async Task<BuildResult?> GetBuildResultAsync(string projectName, int buildId)
        {
            if (string.IsNullOrWhiteSpace(projectName))
                throw new ArgumentException("Project name cannot be null or empty", nameof(projectName));

            if (buildId <= 0)
                throw new ArgumentException("Build ID must be greater than 0", nameof(buildId));

            try
            {
                using var buildClient = _connection.GetClient<BuildHttpClient>();

                var build = await buildClient.GetBuildAsync(projectName, buildId);
                return build?.Result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to get build result for build {buildId} in project {projectName}", ex);
            }
        }

        /// <summary>
        /// Gets a summary of the latest build for a pull request
        /// </summary>
        /// <param name="projectName">The name of the Azure DevOps project</param>
        /// <param name="repositoryId">The ID or name of the repository</param>
        /// <param name="pullRequestId">The ID of the pull request</param>
        /// <returns>Build summary information</returns>
        public async Task<PullRequestBuildSummary?> GetLatestBuildSummaryAsync(
            string projectName,
            string repositoryId,
            int pullRequestId)
        {
            if (string.IsNullOrWhiteSpace(projectName))
                throw new ArgumentException("Project name cannot be null or empty", nameof(projectName));

            if (string.IsNullOrWhiteSpace(repositoryId))
                throw new ArgumentException("Repository ID cannot be null or empty", nameof(repositoryId));

            if (pullRequestId <= 0)
                throw new ArgumentException("Pull request ID must be greater than 0", nameof(pullRequestId));

            try
            {
                var latestBuild = await GetLatestBuildAsync(projectName, repositoryId, pullRequestId);

                if (latestBuild == null)
                    return null;

                return new PullRequestBuildSummary
                {
                    BuildId = latestBuild.Id,
                    BuildNumber = latestBuild.BuildNumber,
                    Status = latestBuild.Status?.ToString(),
                    Result = latestBuild.Result?.ToString(),
                    StartTime = latestBuild.StartTime,
                    FinishTime = latestBuild.FinishTime,
                    SourceBranch = latestBuild.SourceBranch,
                    SourceVersion = latestBuild.SourceVersion,
                    RequestedBy = latestBuild.RequestedBy?.DisplayName,
                    Definition = latestBuild.Definition?.Name,
                    QueueTime = latestBuild.QueueTime,
                    Uri = latestBuild.Uri?.ToString()
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to get build summary for pull request {pullRequestId} in repository {repositoryId}", ex);
            }
        }

        /// <summary>
        /// Exports build information to JSON format
        /// </summary>
        /// <param name="builds">The builds to export</param>
        /// <param name="filePath">Path to save the JSON file</param>
        public async Task ExportBuildsToJsonAsync(IEnumerable<Build> builds, string filePath)
        {
            if (builds == null)
                throw new ArgumentNullException(nameof(builds));

            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonConvert.SerializeObject(builds, Formatting.Indented);
            await File.WriteAllTextAsync(filePath, json, Encoding.UTF8);
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }

    /// <summary>
    /// Represents a simplified build summary for a pull request
    /// </summary>
    public class PullRequestBuildSummary
    {
        public int BuildId { get; set; }
        public string? BuildNumber { get; set; }
        public string? Status { get; set; }
        public string? Result { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? FinishTime { get; set; }
        public DateTime? QueueTime { get; set; }
        public string? SourceBranch { get; set; }
        public string? SourceVersion { get; set; }
        public string? RequestedBy { get; set; }
        public string? Definition { get; set; }
        public string? Uri { get; set; }
    }
}
