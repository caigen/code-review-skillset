using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace Common
{
    public class PullRequestHelper : IDisposable
    {
        private readonly string _organizationUrl;
        private readonly string _personalAccessToken;
        private readonly VssConnection _connection;

        public PullRequestHelper(string organizationUrl, string personalAccessToken)
        {
            _organizationUrl = organizationUrl ?? throw new ArgumentNullException(nameof(organizationUrl));
            _personalAccessToken = personalAccessToken ?? throw new ArgumentNullException(nameof(personalAccessToken));
            
            var credentials = new VssBasicCredential(string.Empty, _personalAccessToken);
            _connection = new VssConnection(new Uri(_organizationUrl), credentials);
        }

        /// <summary>
        /// Gets pull request details from Azure DevOps
        /// </summary>
        /// <param name="projectName">The name of the Azure DevOps project</param>
        /// <param name="repositoryId">The ID or name of the repository</param>
        /// <param name="pullRequestId">The ID of the pull request</param>
        /// <returns>Pull request details, or null if not found</returns>
        public async Task<GitPullRequest?> GetPullRequestAsync(
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

                var pullRequest = await gitClient.GetPullRequestAsync(
                    projectName,
                    repositoryId,
                    pullRequestId);

                return pullRequest;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to get pull request {pullRequestId} from repository {repositoryId} in project {projectName}", ex);
            }
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}
