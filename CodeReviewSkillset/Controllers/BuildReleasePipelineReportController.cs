using Microsoft.AspNetCore.Mvc;
using Common;

namespace GithubSkillsetSample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BuildReleasePipelineReportController : ControllerBase
    {
        private readonly ILogger<BuildReleasePipelineReportController> _logger;

        public BuildReleasePipelineReportController(ILogger<BuildReleasePipelineReportController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Get a detailed build report for a specific build
        /// </summary>
        /// <param name="organizationUrl">Azure DevOps organization URL</param>
        /// <param name="personalAccessToken">Personal access token for authentication</param>
        /// <param name="projectName">Project name</param>
        /// <param name="buildId">Build ID</param>
        /// <returns>Detailed build report</returns>
        [HttpGet]
        public async Task<ActionResult<BuildReport>> GetBuildReport(
            //[FromQuery] string organizationUrl,
            //[FromQuery] string personalAccessToken,
            //[FromQuery] string projectName,
            int? id)
        {
            try
            {
                if (id == null)
                {
                    return BadRequest("Build ID is required");
                }

                // hard coded some parameters for simplicity
                // This test would be used in an integration test environment
                // with actual Azure DevOps organization, project, and build IDs
                var organizationUrl = "https://dev.azure.com/skype";
                var personalAccessToken = Environment.GetEnvironmentVariable("AZURE_DEVOPS_PAT");
                var projectName = "SCC";

                if (string.IsNullOrEmpty(personalAccessToken))
                {
                    return BadRequest("personalAccessToken and projectName are required parameters");
                }

                using var buildReportHelper = new BuildReportHelper(organizationUrl, personalAccessToken);
                var report = await buildReportHelper.GetBuildReportAsync(projectName, id ?? 0);
                
                _logger.LogInformation("Build report retrieved for Build ID: {BuildId} in Project: {ProjectName}", id, projectName);
                
                return Ok(report);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid argument provided for build report request");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving build report for Build ID: {BuildId}", id);
                return StatusCode(500, "An error occurred while retrieving the build report");
            }
        }
    }
}
