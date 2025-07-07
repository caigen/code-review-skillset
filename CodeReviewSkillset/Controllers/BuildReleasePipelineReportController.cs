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
        [Route("build/{buildId:int}")]
        public async Task<ActionResult<BuildReport>> GetBuildReport(
            [FromQuery] string organizationUrl,
            [FromQuery] string personalAccessToken,
            [FromQuery] string projectName,
            int buildId)
        {
            try
            {
                if (string.IsNullOrEmpty(organizationUrl) || string.IsNullOrEmpty(personalAccessToken) || string.IsNullOrEmpty(projectName))
                {
                    return BadRequest("organizationUrl, personalAccessToken, and projectName are required parameters");
                }

                using var buildReportHelper = new BuildReportHelper(organizationUrl, personalAccessToken);
                var report = await buildReportHelper.GetBuildReportAsync(projectName, buildId);
                
                _logger.LogInformation("Build report retrieved for Build ID: {BuildId} in Project: {ProjectName}", buildId, projectName);
                
                return Ok(report);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid argument provided for build report request");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving build report for Build ID: {BuildId}", buildId);
                return StatusCode(500, "An error occurred while retrieving the build report");
            }
        }

        /// <summary>
        /// Get a summary report for multiple builds within a date range
        /// </summary>
        /// <param name="organizationUrl">Azure DevOps organization URL</param>
        /// <param name="personalAccessToken">Personal access token for authentication</param>
        /// <param name="projectName">Project name</param>
        /// <param name="fromDate">Start date for the report</param>
        /// <param name="toDate">End date for the report</param>
        /// <param name="pipelineDefinitionId">Optional pipeline definition ID to filter by</param>
        /// <returns>Summary report for builds</returns>
        [HttpGet]
        [Route("summary")]
        public async Task<ActionResult<BuildSummaryReport>> GetBuildSummaryReport(
            [FromQuery] string organizationUrl,
            [FromQuery] string personalAccessToken,
            [FromQuery] string projectName,
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            [FromQuery] int? pipelineDefinitionId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(organizationUrl) || string.IsNullOrEmpty(personalAccessToken) || string.IsNullOrEmpty(projectName))
                {
                    return BadRequest("organizationUrl, personalAccessToken, and projectName are required parameters");
                }

                if (fromDate >= toDate)
                {
                    return BadRequest("fromDate must be earlier than toDate");
                }

                using var buildReportHelper = new BuildReportHelper(organizationUrl, personalAccessToken);
                var report = await buildReportHelper.GetBuildSummaryReportAsync(projectName, fromDate, toDate, pipelineDefinitionId);
                
                _logger.LogInformation("Build summary report retrieved for Project: {ProjectName} from {FromDate} to {ToDate}", 
                    projectName, fromDate, toDate);
                
                return Ok(report);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid argument provided for build summary report request");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving build summary report for Project: {ProjectName}", projectName);
                return StatusCode(500, "An error occurred while retrieving the build summary report");
            }
        }

        /// <summary>
        /// Get a list of recent builds for a specific pipeline
        /// </summary>
        /// <param name="organizationUrl">Azure DevOps organization URL</param>
        /// <param name="personalAccessToken">Personal access token for authentication</param>
        /// <param name="projectName">Project name</param>
        /// <param name="pipelineDefinitionId">Pipeline definition ID (required)</param>
        /// <param name="count">Number of recent builds to retrieve (default: 10)</param>
        /// <returns>List of recent build reports</returns>
        [HttpGet]
        [Route("recent")]
        public async Task<ActionResult<IEnumerable<BuildReport>>> GetRecentBuilds(
            [FromQuery] string organizationUrl,
            [FromQuery] string personalAccessToken,
            [FromQuery] string projectName,
            [FromQuery] int pipelineDefinitionId,
            [FromQuery] int count = 10)
        {
            try
            {
                if (string.IsNullOrEmpty(organizationUrl) || string.IsNullOrEmpty(personalAccessToken) || string.IsNullOrEmpty(projectName))
                {
                    return BadRequest("organizationUrl, personalAccessToken, and projectName are required parameters");
                }

                if (pipelineDefinitionId <= 0)
                {
                    return BadRequest("pipelineDefinitionId must be greater than 0");
                }

                if (count <= 0 || count > 100)
                {
                    return BadRequest("count must be between 1 and 100");
                }

                using var buildReportHelper = new BuildReportHelper(organizationUrl, personalAccessToken);
                var builds = await buildReportHelper.GetRecentBuildReportsAsync(projectName, pipelineDefinitionId, count);
                
                _logger.LogInformation("Retrieved {Count} recent builds for Project: {ProjectName}, Pipeline: {PipelineDefinitionId}", 
                    builds.Count(), projectName, pipelineDefinitionId);
                
                return Ok(builds);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid argument provided for recent builds request");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent builds for Project: {ProjectName}", projectName);
                return StatusCode(500, "An error occurred while retrieving recent builds");
            }
        }

        /// <summary>
        /// Get detailed build reports for multiple builds
        /// </summary>
        /// <param name="organizationUrl">Azure DevOps organization URL</param>
        /// <param name="personalAccessToken">Personal access token for authentication</param>
        /// <param name="projectName">Project name</param>
        /// <param name="buildIds">Comma-separated list of build IDs</param>
        /// <returns>List of detailed build reports</returns>
        [HttpGet]
        [Route("multiple")]
        public async Task<ActionResult<IEnumerable<BuildReport>>> GetMultipleBuildReports(
            [FromQuery] string organizationUrl,
            [FromQuery] string personalAccessToken,
            [FromQuery] string projectName,
            [FromQuery] string buildIds)
        {
            try
            {
                if (string.IsNullOrEmpty(organizationUrl) || string.IsNullOrEmpty(personalAccessToken) || string.IsNullOrEmpty(projectName))
                {
                    return BadRequest("organizationUrl, personalAccessToken, and projectName are required parameters");
                }

                if (string.IsNullOrEmpty(buildIds))
                {
                    return BadRequest("buildIds parameter is required");
                }

                // Parse the comma-separated build IDs
                var buildIdList = new List<int>();
                var idStrings = buildIds.Split(',', StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var idString in idStrings)
                {
                    if (int.TryParse(idString.Trim(), out var buildId))
                    {
                        buildIdList.Add(buildId);
                    }
                    else
                    {
                        return BadRequest($"Invalid build ID: {idString}");
                    }
                }

                if (buildIdList.Count == 0)
                {
                    return BadRequest("At least one valid build ID must be provided");
                }

                if (buildIdList.Count > 50)
                {
                    return BadRequest("Maximum of 50 build IDs can be requested at once");
                }

                using var buildReportHelper = new BuildReportHelper(organizationUrl, personalAccessToken);
                var reports = await buildReportHelper.GetMultipleBuildReportsAsync(projectName, buildIdList);
                
                _logger.LogInformation("Retrieved {Count} build reports for Project: {ProjectName}", reports.Count(), projectName);
                
                return Ok(reports);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid argument provided for multiple build reports request");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving multiple build reports for Project: {ProjectName}", projectName);
                return StatusCode(500, "An error occurred while retrieving multiple build reports");
            }
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        /// <returns>API status</returns>
        [HttpGet]
        [Route("health")]
        public ActionResult<object> Health()
        {
            return Ok(new { 
                status = "healthy", 
                timestamp = DateTime.UtcNow,
                controller = "BuildReleasePipelineReportController"
            });
        }
    }
}
