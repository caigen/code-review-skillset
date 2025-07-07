# PullRequestBuildHelper

A helper class for retrieving build information associated with Azure DevOps pull requests using the Azure DevOps REST API.

## Overview

The `PullRequestBuildHelper` class provides methods to:
- Get the latest build ID for a pull request
- Retrieve detailed build information
- Get build status and results
- Export build data to JSON format

## Prerequisites

- Azure DevOps Personal Access Token (PAT) with appropriate permissions:
  - Build (read)
  - Code (read)
- .NET 8.0 or later

## Installation

Add the following NuGet packages to your project:

```xml
<PackageReference Include="Microsoft.TeamFoundationServer.Client" Version="19.225.1" />
<PackageReference Include="Microsoft.VisualStudio.Services.Client" Version="19.225.1" />
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
```

## Usage Examples

### Basic Setup

```csharp
using Common;

// Initialize the helper
var organizationUrl = "https://dev.azure.com/your-organization";
var personalAccessToken = "your-pat-token";

using var buildHelper = new PullRequestBuildHelper(organizationUrl, personalAccessToken);
```

### Get Latest Build ID

```csharp
// Get the latest build ID for a pull request
var buildId = await buildHelper.GetLatestBuildIdAsync(
    projectName: "MyProject",
    repositoryId: "MyRepository", 
    pullRequestId: 123);

if (buildId.HasValue)
{
    Console.WriteLine($"Latest build ID: {buildId.Value}");
}
else
{
    Console.WriteLine("No builds found for this pull request");
}
```

### Get Latest Build Details

```csharp
// Get complete build information
var build = await buildHelper.GetLatestBuildAsync(
    projectName: "MyProject",
    repositoryId: "MyRepository",
    pullRequestId: 123);

if (build != null)
{
    Console.WriteLine($"Build ID: {build.Id}");
    Console.WriteLine($"Build Number: {build.BuildNumber}");
    Console.WriteLine($"Status: {build.Status}");
    Console.WriteLine($"Result: {build.Result}");
    Console.WriteLine($"Start Time: {build.StartTime}");
    Console.WriteLine($"Finish Time: {build.FinishTime}");
}
```

### Get Build Summary

```csharp
// Get a simplified build summary
var summary = await buildHelper.GetLatestBuildSummaryAsync(
    projectName: "MyProject",
    repositoryId: "MyRepository",
    pullRequestId: 123);

if (summary != null)
{
    Console.WriteLine($"Build {summary.BuildNumber} - {summary.Status}");
    Console.WriteLine($"Result: {summary.Result}");
    Console.WriteLine($"Requested by: {summary.RequestedBy}");
}
```

### Get All Builds for a Pull Request

```csharp
// Get multiple builds for a pull request
var builds = await buildHelper.GetPullRequestBuildsAsync(
    projectName: "MyProject",
    repositoryId: "MyRepository",
    pullRequestId: 123,
    maxBuilds: 5);

foreach (var build in builds)
{
    Console.WriteLine($"Build {build.BuildNumber}: {build.Status} - {build.Result}");
}
```

### Get Build Status and Result

```csharp
// Get status for a specific build
var status = await buildHelper.GetBuildStatusAsync("MyProject", 456);
Console.WriteLine($"Build Status: {status}");

// Get result for a specific build
var result = await buildHelper.GetBuildResultAsync("MyProject", 456);
Console.WriteLine($"Build Result: {result}");
```

### Export Build Data

```csharp
// Export builds to JSON file
var builds = await buildHelper.GetPullRequestBuildsAsync(
    "MyProject", "MyRepository", 123, 10);

await buildHelper.ExportBuildsToJsonAsync(builds, @"C:\temp\builds.json");
```

## API Methods

### Core Methods

| Method | Description | Returns |
|--------|-------------|---------|
| `GetLatestBuildIdAsync` | Gets the latest build ID for a PR | `int?` |
| `GetLatestBuildAsync` | Gets the latest build with full details | `Build?` |
| `GetPullRequestBuildsAsync` | Gets all builds for a PR | `IEnumerable<Build>` |
| `GetLatestBuildSummaryAsync` | Gets a simplified build summary | `PullRequestBuildSummary?` |

### Utility Methods

| Method | Description | Returns |
|--------|-------------|---------|
| `GetBuildStatusAsync` | Gets status of a specific build | `BuildStatus?` |
| `GetBuildResultAsync` | Gets result of a specific build | `BuildResult?` |
| `ExportBuildsToJsonAsync` | Exports builds to JSON file | `Task` |

## Build Status Values

- `InProgress` - Build is currently running
- `Completed` - Build has finished
- `Cancelling` - Build is being cancelled
- `Postponed` - Build has been postponed
- `NotStarted` - Build has not started yet

## Build Result Values

- `None` - No result yet
- `Succeeded` - Build succeeded
- `PartiallySucceeded` - Build partially succeeded
- `Failed` - Build failed
- `Canceled` - Build was canceled

## Error Handling

All methods include comprehensive error handling and will throw `InvalidOperationException` with descriptive messages if:
- The pull request is not found
- Network or authentication issues occur
- Invalid parameters are provided

## Permissions Required

Your Personal Access Token must have the following scopes:
- **Build (read)** - To access build information
- **Code (read)** - To access pull request details

## Notes

- The helper automatically gets the source branch from the pull request to find associated builds
- Builds are returned in descending order by finish time (most recent first)
- The helper implements `IDisposable` for proper resource cleanup
- All string parameters are validated for null/empty values
- Numeric parameters are validated for positive values

## Example Integration

```csharp
public class BuildReportService
{
    private readonly PullRequestBuildHelper _buildHelper;

    public BuildReportService(string organizationUrl, string pat)
    {
        _buildHelper = new PullRequestBuildHelper(organizationUrl, pat);
    }

    public async Task<string> GenerateBuildReportAsync(
        string project, string repo, int prId)
    {
        var summary = await _buildHelper.GetLatestBuildSummaryAsync(
            project, repo, prId);

        if (summary == null)
            return "No builds found for this pull request.";

        return $"""
            Latest Build Report
            ===================
            Build: {summary.BuildNumber}
            Status: {summary.Status}
            Result: {summary.Result}
            Started: {summary.StartTime}
            Finished: {summary.FinishTime}
            Requested by: {summary.RequestedBy}
            """;
    }
}
```
