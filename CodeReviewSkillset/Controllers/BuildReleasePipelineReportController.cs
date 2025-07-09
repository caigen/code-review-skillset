using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.TeamFoundation.Build.WebApi;
using Newtonsoft.Json;

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

        [HttpGet]
        [Route("list")]
        public async Task<IEnumerable<BuildReport>> List()
        {
            // hard coded some parameters for simplicity
            var realOrganizationUrl = "https://dev.azure.com/skype";
            var realPat = Environment.GetEnvironmentVariable("AZURE_DEVOPS_PAT");
            var realProjectName = "SCC";

            List<BuildReport> buildReports = new List<BuildReport>();
            if (string.IsNullOrEmpty(realPat))
            {
                return buildReports; // Return empty list if PAT is not set
            }

            using var realBuildReportHelper = new BuildReportHelper(realOrganizationUrl, realPat);

            List<string> buildId = new List<string>
            {
                "71015951", // Example build ID
                "70997686"  // one release pipeline
            };

            // Loop through each build ID to get reports
            foreach (var build in buildId)
            {
                var report = await realBuildReportHelper.GetBuildReportAsync(realProjectName, int.Parse(build));
                buildReports.Add(report);
            }

            return buildReports;
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
        public async Task<ActionResult<IEnumerable<BuildReport>>> GetBuildReport(
            //[FromQuery] string organizationUrl,
            //[FromQuery] string personalAccessToken,
            //[FromQuery] string projectName,
            int? runId)
        {
            try
            {
                if (runId == null)
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
                BuildReport report = await buildReportHelper.GetBuildReportAsync(projectName, runId ?? 0);

                _logger.LogInformation("Build report retrieved for Build ID: {BuildId} in Project: {ProjectName}", runId, projectName);

                return Ok(new List<BuildReport> { report });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid argument provided for build report request");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving build report for Build ID: {BuildId}", runId);
                return StatusCode(500, "An error occurred while retrieving the build report");
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BuildReport>>> GetRecentBuildReportsAsync(
            int? pipelineDefinitionId)
        {
            try
            {
                if (pipelineDefinitionId == null)
                {
                    return BadRequest("Pipeline Definition ID is required");
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
                var report = await buildReportHelper.GetRecentBuildReportsAsync(projectName, pipelineDefinitionId ?? 0);

                _logger.LogInformation("Recent build reports retrieved for Pipeline Definition ID: {PipelineDefinitionId} in Project: {ProjectName}", pipelineDefinitionId, projectName);

                return Ok(report);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid argument provided for build report request");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving build reports for Pipeline Definition ID: {PipelineDefinitionId}", pipelineDefinitionId);
                return StatusCode(500, "An error occurred while retrieving the recent build reports");
            }
        }

        [HttpPost]
        public async Task<ActionResult<IEnumerable<BuildReport>>> Post([FromBody] SkillsetRequest request)
        {
            Console.WriteLine("Post called");

            if (request.RunId != null)
            {
                return await this.GetBuildReport(
                    //organizationUrl: request.OrganizationUrl,
                    //personalAccessToken: request.PersonalAccessToken,
                    //projectName: request.ProjectName,
                    runId: request.Id != null ? int.Parse(request.Id) : (int?)null
                );
            }

            else if (request.PipelineId != null)
            {
                return await this.GetRecentBuildReportsAsync(
                    pipelineDefinitionId: request.PipelineId != null ? int.Parse(request.PipelineId) : (int?)null
                );
            }
            else
            {
                return BadRequest("RunId or PipelineId is required");
            }
        }
    }
}