using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace GithubSkillsetSample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PullRequestController : ControllerBase
    {
        private readonly ILogger<PullRequestController> _logger;

        public PullRequestController(ILogger<PullRequestController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Get pull request details by ID
        /// </summary>
        /// <param name="id">Pull request ID</param>
        /// <returns>Pull request details</returns>
        [HttpGet]
        public async Task<ActionResult<GitPullRequest>> GetPullRequest(int? id)
        {
            try
            {
                if (id == null || id <= 0)
                {
                    return BadRequest("Pull request ID is required and must be greater than 0");
                }

                // Hard coded some parameters for simplicity
                // This test would be used in an integration test environment
                // with actual Azure DevOps organization, project, and repository details
                var organizationUrl = "https://dev.azure.com/skype";
                var personalAccessToken = Environment.GetEnvironmentVariable("AZURE_DEVOPS_PAT");
                var projectName = "SCC";
                var repositoryId = "sync_calling_concore-conversation"; // Using project name as repository ID, can be customized

                if (string.IsNullOrEmpty(personalAccessToken))
                {
                    return BadRequest("Personal Access Token (AZURE_DEVOPS_PAT) environment variable is required");
                }

                using var pullRequestHelper = new PullRequestHelper(organizationUrl, personalAccessToken);
                var pullRequest = await pullRequestHelper.GetPullRequestAsync(projectName, repositoryId, id.Value);
                
                if (pullRequest == null)
                {
                    return NotFound($"Pull request with ID {id} not found");
                }

                _logger.LogInformation("Pull request retrieved for PR ID: {PullRequestId} in Project: {ProjectName}", id, projectName);
                
                return Ok(pullRequest);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid argument provided for pull request");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error retrieving pull request for PR ID: {PullRequestId}", id);
                return StatusCode(500, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving pull request for PR ID: {PullRequestId}", id);
                return StatusCode(500, "An unexpected error occurred while retrieving the pull request");
            }
        }

        /// <summary>
        /// Get pull request details using POST request with SkillsetRequest body
        /// </summary>
        /// <param name="request">Request containing pull request ID</param>
        /// <returns>Pull request details</returns>
        [HttpPost]
        public async Task<ActionResult<GitPullRequest>> Post([FromBody] SkillsetRequest request)
        {
            Console.WriteLine("Post called for PullRequest");

            if (request == null || string.IsNullOrEmpty(request.PullRequestId))
            {
                return BadRequest("Pull request ID is required in the request body");
            }

            return await this.GetPullRequest(
                id: request.PullRequestId != null ? int.Parse(request.PullRequestId) : (int?)null
            );
        }
    }
}
