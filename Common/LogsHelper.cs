using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json;
using System.Text;

namespace Common
{
    public class LogsHelper
    {
        private readonly string _organizationUrl;
        private readonly string _personalAccessToken;
        private readonly VssConnection _connection;

        public LogsHelper(string organizationUrl, string personalAccessToken)
        {
            _organizationUrl = organizationUrl ?? throw new ArgumentNullException(nameof(organizationUrl));
            _personalAccessToken = personalAccessToken ?? throw new ArgumentNullException(nameof(personalAccessToken));
            
            var credentials = new VssBasicCredential(string.Empty, _personalAccessToken);
            _connection = new VssConnection(new Uri(_organizationUrl), credentials);
        }

        /// <summary>
        /// Gets the logs for a specific pipeline run
        /// </summary>
        /// <param name="projectName">The name of the Azure DevOps project</param>
        /// <param name="buildId">The ID of the build/pipeline run</param>
        /// <returns>A collection of log entries</returns>
        public async Task<IEnumerable<BuildLogEntry>> GetPipelineRunLogsAsync(string projectName, int buildId)
        {
            if (string.IsNullOrWhiteSpace(projectName))
                throw new ArgumentException("Project name cannot be null or empty", nameof(projectName));

            if (buildId <= 0)
                throw new ArgumentException("Build ID must be greater than 0", nameof(buildId));

            try
            {
                using var buildClient = _connection.GetClient<BuildHttpClient>();
                
                // Get the build logs
                var logs = await buildClient.GetBuildLogsAsync(projectName, buildId);
                
                var logEntries = new List<BuildLogEntry>();
                
                foreach (var log in logs)
                {
                    if (log.Id > 0)
                    {
                        var logContent = await GetLogContentAsync(buildClient, projectName, buildId, log.Id);
                        logEntries.Add(new BuildLogEntry
                        {
                            Id = log.Id,
                            Type = log.Type,
                            Url = log.Url,
                            Content = logContent,
                            LineCount = (int)log.LineCount,
                            CreatedOn = log.CreatedOn,
                            LastChangedOn = log.LastChangedOn
                        });
                    }
                }
                
                return logEntries;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve pipeline logs for build {buildId} in project {projectName}", ex);
            }
        }

        /// <summary>
        /// Gets the logs for a specific pipeline run as a formatted string
        /// </summary>
        /// <param name="projectName">The name of the Azure DevOps project</param>
        /// <param name="buildId">The ID of the build/pipeline run</param>
        /// <returns>Formatted log string</returns>
        public async Task<string> GetPipelineRunLogsAsStringAsync(string projectName, int buildId)
        {
            var logs = await GetPipelineRunLogsAsync(projectName, buildId);
            
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"=== Pipeline Run Logs for Build {buildId} ===");
            stringBuilder.AppendLine($"Project: {projectName}");
            stringBuilder.AppendLine($"Retrieved on: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            stringBuilder.AppendLine();
            
            foreach (var log in logs.OrderBy(l => l.Id))
            {
                stringBuilder.AppendLine($"--- Log {log.Id} ({log.Type}) ---");
                stringBuilder.AppendLine($"Created: {log.CreatedOn:yyyy-MM-dd HH:mm:ss}");
                stringBuilder.AppendLine($"Lines: {log.LineCount}");
                stringBuilder.AppendLine();
                stringBuilder.AppendLine(log.Content);
                stringBuilder.AppendLine();
            }
            
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Gets logs for the most recent pipeline runs for a specific pipeline definition
        /// </summary>
        /// <param name="projectName">The name of the Azure DevOps project</param>
        /// <param name="pipelineDefinitionId">The ID of the pipeline definition</param>
        /// <param name="count">Number of recent runs to retrieve logs for (default: 5)</param>
        /// <returns>Dictionary with build ID as key and logs as value</returns>
        public async Task<Dictionary<int, IEnumerable<BuildLogEntry>>> GetRecentPipelineRunLogsAsync(
            string projectName, 
            int pipelineDefinitionId, 
            int count = 5)
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
                    queryOrder: BuildQueryOrder.FinishTimeDescending
                );

                var result = new Dictionary<int, IEnumerable<BuildLogEntry>>();
                
                foreach (var build in builds)
                {
                    var logs = await GetPipelineRunLogsAsync(projectName, build.Id);
                    result[build.Id] = logs;
                }
                
                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve recent pipeline logs for definition {pipelineDefinitionId} in project {projectName}", ex);
            }
        }

        /// <summary>
        /// Saves pipeline logs to a file
        /// </summary>
        /// <param name="projectName">The name of the Azure DevOps project</param>
        /// <param name="buildId">The ID of the build/pipeline run</param>
        /// <param name="filePath">Path to save the logs file</param>
        public async Task SavePipelineLogsToFileAsync(string projectName, int buildId, string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

            var logsContent = await GetPipelineRunLogsAsStringAsync(projectName, buildId);
            
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            await File.WriteAllTextAsync(filePath, logsContent, Encoding.UTF8);
        }

        private async Task<string> GetLogContentAsync(BuildHttpClient buildClient, string projectName, int buildId, int logId)
        {
            try
            {
                using var logStream = await buildClient.GetBuildLogAsync(projectName, buildId, logId);
                using var reader = new StreamReader(logStream);
                return await reader.ReadToEndAsync();
            }
            catch (Exception ex)
            {
                return $"Error retrieving log content for log {logId}: {ex.Message}";
            }
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }

    /// <summary>
    /// Represents a build log entry with content and metadata
    /// </summary>
    public class BuildLogEntry
    {
        public int Id { get; set; }
        public string? Type { get; set; }
        public string? Url { get; set; }
        public string Content { get; set; } = string.Empty;
        public int LineCount { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? LastChangedOn { get; set; }
    }
}
