# Common Library

This library provides utilities for working with Azure DevOps, including pipeline log retrieval.

## Azure DevOps Pipeline Logs

The `LogsHelper` class provides functionality to retrieve logs from Azure DevOps pipeline runs.

### Setup

To use the Azure DevOps functionality, you'll need:

1. **Azure DevOps Organization URL**: The URL of your Azure DevOps organization (e.g., `https://dev.azure.com/yourorganization`)
2. **Personal Access Token (PAT)**: A PAT with appropriate permissions to read build logs

### Creating a Personal Access Token

1. Go to your Azure DevOps organization
2. Click on User Settings (profile picture) → Personal Access Tokens
3. Create a new token with the following scopes:
   - **Build (read)** - to read pipeline information
   - **Code (read)** - if you need to access repository information

### Usage Examples

```csharp
using Common;

// Initialize the LogsHelper
var organizationUrl = "https://dev.azure.com/yourorganization";
var personalAccessToken = "your-pat-here";
var logsHelper = new LogsHelper(organizationUrl, personalAccessToken);

// Get logs for a specific pipeline run
var projectName = "YourProject";
var buildId = 12345;
var logs = await logsHelper.GetPipelineRunLogsAsync(projectName, buildId);

// Get logs as a formatted string
var logsString = await logsHelper.GetPipelineRunLogsAsStringAsync(projectName, buildId);
Console.WriteLine(logsString);

// Get logs for recent pipeline runs
var pipelineDefinitionId = 123;
var recentLogs = await logsHelper.GetRecentPipelineRunLogsAsync(projectName, pipelineDefinitionId, 5);

// Save logs to a file
var filePath = @"C:\temp\pipeline-logs.txt";
await logsHelper.SavePipelineLogsToFileAsync(projectName, buildId, filePath);

// Don't forget to dispose when done
logsHelper.Dispose();
```

### Methods Available

- **`GetPipelineRunLogsAsync`**: Gets logs for a specific pipeline run
- **`GetPipelineRunLogsAsStringAsync`**: Gets logs formatted as a single string
- **`GetRecentPipelineRunLogsAsync`**: Gets logs for recent runs of a pipeline definition
- **`SavePipelineLogsToFileAsync`**: Saves logs to a file

### Error Handling

All methods include proper error handling and will throw meaningful exceptions if:
- Invalid parameters are provided
- Authentication fails
- The pipeline run or project doesn't exist
- Network issues occur

### Dependencies

This library uses the following NuGet packages:
- `Microsoft.TeamFoundationServer.Client` - Azure DevOps REST API client
- `Microsoft.VisualStudio.Services.Client` - Visual Studio Team Services client
- `Newtonsoft.Json` - JSON serialization

### Security Notes

- Store your Personal Access Token securely (environment variables, Azure Key Vault, etc.)
- Use the principle of least privilege when creating PATs
- Regularly rotate your access tokens
