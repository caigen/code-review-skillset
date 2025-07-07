using Common;

namespace Common.Examples
{
    /// <summary>
    /// Example usage of PullRequestBuildHelper
    /// </summary>
    public class PullRequestBuildExample
    {
        private readonly string _organizationUrl;
        private readonly string _personalAccessToken;

        public PullRequestBuildExample(string organizationUrl, string personalAccessToken)
        {
            _organizationUrl = organizationUrl;
            _personalAccessToken = personalAccessToken;
        }

        /// <summary>
        /// Example: Get latest build ID for a pull request
        /// </summary>
        public async Task<int?> GetLatestBuildIdExample(
            string projectName, 
            string repositoryId, 
            int pullRequestId)
        {
            using var buildHelper = new PullRequestBuildHelper(_organizationUrl, _personalAccessToken);

            try
            {
                var buildId = await buildHelper.GetLatestBuildIdAsync(
                    projectName, 
                    repositoryId, 
                    pullRequestId);

                if (buildId.HasValue)
                {
                    Console.WriteLine($"✅ Latest build ID: {buildId.Value}");
                    return buildId.Value;
                }
                else
                {
                    Console.WriteLine("⚠️ No builds found for this pull request");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error getting build ID: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Example: Get comprehensive build information
        /// </summary>
        public async Task GetBuildDetailsExample(
            string projectName, 
            string repositoryId, 
            int pullRequestId)
        {
            using var buildHelper = new PullRequestBuildHelper(_organizationUrl, _personalAccessToken);

            try
            {
                var summary = await buildHelper.GetLatestBuildSummaryAsync(
                    projectName, 
                    repositoryId, 
                    pullRequestId);

                if (summary != null)
                {
                    Console.WriteLine("📊 Build Summary:");
                    Console.WriteLine($"   Build ID: {summary.BuildId}");
                    Console.WriteLine($"   Build Number: {summary.BuildNumber}");
                    Console.WriteLine($"   Status: {summary.Status}");
                    Console.WriteLine($"   Result: {summary.Result}");
                    Console.WriteLine($"   Definition: {summary.Definition}");
                    Console.WriteLine($"   Requested by: {summary.RequestedBy}");
                    Console.WriteLine($"   Queue Time: {summary.QueueTime}");
                    Console.WriteLine($"   Start Time: {summary.StartTime}");
                    Console.WriteLine($"   Finish Time: {summary.FinishTime}");
                    Console.WriteLine($"   Source Branch: {summary.SourceBranch}");
                    Console.WriteLine($"   Build URI: {summary.Uri}");
                }
                else
                {
                    Console.WriteLine("⚠️ No build information available");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error getting build details: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Example: Monitor build status until completion
        /// </summary>
        public async Task MonitorBuildStatusExample(
            string projectName, 
            string repositoryId, 
            int pullRequestId)
        {
            using var buildHelper = new PullRequestBuildHelper(_organizationUrl, _personalAccessToken);

            try
            {
                var buildId = await buildHelper.GetLatestBuildIdAsync(
                    projectName, 
                    repositoryId, 
                    pullRequestId);

                if (!buildId.HasValue)
                {
                    Console.WriteLine("⚠️ No builds found to monitor");
                    return;
                }

                Console.WriteLine($"🔍 Monitoring build {buildId.Value}...");

                while (true)
                {
                    var status = await buildHelper.GetBuildStatusAsync(projectName, buildId.Value);
                    var result = await buildHelper.GetBuildResultAsync(projectName, buildId.Value);

                    Console.WriteLine($"   Status: {status}, Result: {result}");

                    if (status == Microsoft.TeamFoundation.Build.WebApi.BuildStatus.Completed)
                    {
                        Console.WriteLine($"✅ Build completed with result: {result}");
                        break;
                    }

                    // Wait 30 seconds before checking again
                    await Task.Delay(30000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error monitoring build: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Example: Export build history to JSON
        /// </summary>
        public async Task ExportBuildHistoryExample(
            string projectName, 
            string repositoryId, 
            int pullRequestId,
            string outputPath)
        {
            using var buildHelper = new PullRequestBuildHelper(_organizationUrl, _personalAccessToken);

            try
            {
                Console.WriteLine("📥 Fetching build history...");
                
                var builds = await buildHelper.GetPullRequestBuildsAsync(
                    projectName, 
                    repositoryId, 
                    pullRequestId, 
                    maxBuilds: 20);

                Console.WriteLine($"   Found {builds.Count()} builds");

                await buildHelper.ExportBuildsToJsonAsync(builds, outputPath);
                
                Console.WriteLine($"💾 Build history exported to: {outputPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error exporting build history: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Example: Generate a simple build report
        /// </summary>
        public async Task<string> GenerateBuildReportExample(
            string projectName, 
            string repositoryId, 
            int pullRequestId)
        {
            using var buildHelper = new PullRequestBuildHelper(_organizationUrl, _personalAccessToken);

            try
            {
                var builds = await buildHelper.GetPullRequestBuildsAsync(
                    projectName, 
                    repositoryId, 
                    pullRequestId, 
                    maxBuilds: 5);

                if (!builds.Any())
                {
                    return "No builds found for this pull request.";
                }

                var report = $"""
                    Build Report for Pull Request {pullRequestId}
                    ============================================
                    
                    Recent Builds:
                    """;

                foreach (var build in builds)
                {
                    var duration = build.FinishTime.HasValue && build.StartTime.HasValue 
                        ? build.FinishTime.Value - build.StartTime.Value 
                        : TimeSpan.Zero;

                    report += $"""
                        
                        Build {build.BuildNumber}:
                          - ID: {build.Id}
                          - Status: {build.Status}
                          - Result: {build.Result}
                          - Started: {build.StartTime}
                          - Duration: {duration:hh\\:mm\\:ss}
                          - Requested by: {build.RequestedBy?.DisplayName}
                        """;
                }

                Console.WriteLine("📋 Build report generated successfully");
                return report;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error generating build report: {ex.Message}");
                throw;
            }
        }
    }

    /// <summary>
    /// Console application to demonstrate the PullRequestBuildHelper
    /// </summary>
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Configuration - replace with your values
            const string organizationUrl = "https://dev.azure.com/your-organization";
            const string personalAccessToken = "your-pat-token";
            const string projectName = "YourProject";
            const string repositoryId = "YourRepository";
            const int pullRequestId = 123;

            var example = new PullRequestBuildExample(organizationUrl, personalAccessToken);

            try
            {
                Console.WriteLine("🚀 PullRequestBuildHelper Example");
                Console.WriteLine("==================================");

                // Example 1: Get latest build ID
                Console.WriteLine("\n1️⃣ Getting latest build ID...");
                await example.GetLatestBuildIdExample(projectName, repositoryId, pullRequestId);

                // Example 2: Get build details
                Console.WriteLine("\n2️⃣ Getting build details...");
                await example.GetBuildDetailsExample(projectName, repositoryId, pullRequestId);

                // Example 3: Generate build report
                Console.WriteLine("\n3️⃣ Generating build report...");
                var report = await example.GenerateBuildReportExample(projectName, repositoryId, pullRequestId);
                Console.WriteLine(report);

                // Example 4: Export build history
                Console.WriteLine("\n4️⃣ Exporting build history...");
                await example.ExportBuildHistoryExample(
                    projectName, 
                    repositoryId, 
                    pullRequestId, 
                    @"C:\temp\pr-builds.json");

                Console.WriteLine("\n✅ All examples completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Example failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
