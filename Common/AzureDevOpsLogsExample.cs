using Common;

namespace Common.Examples
{
    /// <summary>
    /// Example class demonstrating how to use the LogsHelper for Azure DevOps pipeline logs
    /// </summary>
    public class AzureDevOpsLogsExample
    {
        private readonly LogsHelper _logsHelper;

        public AzureDevOpsLogsExample(string organizationUrl, string personalAccessToken)
        {
            _logsHelper = new LogsHelper(organizationUrl, personalAccessToken);
        }

        /// <summary>
        /// Example: Get and display logs for a specific pipeline run
        /// </summary>
        public async Task GetAndDisplayPipelineLogsAsync(string projectName, int buildId)
        {
            try
            {
                Console.WriteLine($"Retrieving logs for build {buildId} in project {projectName}...");
                
                var logs = await _logsHelper.GetPipelineRunLogsAsync(projectName, buildId);
                
                Console.WriteLine($"Found {logs.Count()} log entries:");
                
                foreach (var log in logs)
                {
                    Console.WriteLine($"\n--- Log {log.Id} ({log.Type}) ---");
                    Console.WriteLine($"Created: {log.CreatedOn}");
                    Console.WriteLine($"Lines: {log.LineCount}");
                    Console.WriteLine("Content:");
                    Console.WriteLine(log.Content.Length > 500 
                        ? log.Content.Substring(0, 500) + "..." 
                        : log.Content);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving logs: {ex.Message}");
            }
        }

        /// <summary>
        /// Example: Save logs to a file
        /// </summary>
        public async Task SaveLogsToFileAsync(string projectName, int buildId, string outputPath)
        {
            try
            {
                Console.WriteLine($"Saving logs for build {buildId} to {outputPath}...");
                
                await _logsHelper.SavePipelineLogsToFileAsync(projectName, buildId, outputPath);
                
                Console.WriteLine("Logs saved successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving logs: {ex.Message}");
            }
        }

        /// <summary>
        /// Example: Get logs for recent pipeline runs
        /// </summary>
        public async Task GetRecentPipelineLogsAsync(string projectName, int pipelineDefinitionId, int count = 3)
        {
            try
            {
                Console.WriteLine($"Retrieving logs for {count} recent runs of pipeline {pipelineDefinitionId}...");
                
                var recentLogs = await _logsHelper.GetRecentPipelineRunLogsAsync(projectName, pipelineDefinitionId, count);
                
                foreach (var buildLogs in recentLogs)
                {
                    Console.WriteLine($"\n=== Build {buildLogs.Key} ===");
                    Console.WriteLine($"Total log entries: {buildLogs.Value.Count()}");
                    
                    // Show just the first log entry summary
                    var firstLog = buildLogs.Value.FirstOrDefault();
                    if (firstLog != null)
                    {
                        Console.WriteLine($"First log created: {firstLog.CreatedOn}");
                        Console.WriteLine($"First log type: {firstLog.Type}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving recent logs: {ex.Message}");
            }
        }

        /// <summary>
        /// Example: Monitor a pipeline run and get logs when it completes
        /// </summary>
        public async Task MonitorPipelineAndGetLogsAsync(string projectName, int buildId, TimeSpan timeout)
        {
            try
            {
                Console.WriteLine($"Monitoring build {buildId} for completion...");
                
                var startTime = DateTime.UtcNow;
                var checkInterval = TimeSpan.FromSeconds(30);
                
                while (DateTime.UtcNow - startTime < timeout)
                {
                    try
                    {
                        // Try to get logs - if successful, the build might be complete
                        var logs = await _logsHelper.GetPipelineRunLogsAsync(projectName, buildId);
                        
                        if (logs.Any())
                        {
                            Console.WriteLine($"Build completed! Retrieved {logs.Count()} log entries.");
                            
                            // Save logs to a timestamped file
                            var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                            var filename = $"pipeline-{buildId}-{timestamp}.log";
                            await _logsHelper.SavePipelineLogsToFileAsync(projectName, buildId, filename);
                            
                            Console.WriteLine($"Logs saved to {filename}");
                            return;
                        }
                    }
                    catch
                    {
                        // Build might still be running, continue monitoring
                    }
                    
                    Console.WriteLine("Build still running, checking again in 30 seconds...");
                    await Task.Delay(checkInterval);
                }
                
                Console.WriteLine("Timeout reached while monitoring build.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error monitoring pipeline: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _logsHelper?.Dispose();
        }
    }
}
