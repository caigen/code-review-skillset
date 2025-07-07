# Pull Request Comment Helper

This helper class provides a comprehensive API for interacting with Azure DevOps pull request comments. It allows you to add comments, reply to existing comments, manage comment thread status, and export comments for analysis.

## Features

- ✅ Add general comments to pull requests
- ✅ Add line-specific comments to files
- ✅ Add multiple comments in batch
- ✅ Reply to existing comment threads
- ✅ Update comment thread status (resolve, reactivate, etc.)
- ✅ Retrieve all comment threads for a pull request
- ✅ Export comments to JSON format
- ✅ Comprehensive error handling and validation

## Prerequisites

- Azure DevOps Personal Access Token (PAT) with appropriate permissions
- .NET 8.0 or later
- Azure DevOps organization URL

## Required Permissions

Your Personal Access Token needs the following permissions:
- **Code (read & write)** - for accessing repositories and pull requests
- **Pull Request Thread (read & write)** - for managing comment threads

## Basic Usage

### Initialize the Helper

```csharp
using Common;

var organizationUrl = "https://dev.azure.com/your-organization";
var personalAccessToken = "your-pat-token";

using var commentHelper = new PullRequestCommentHelper(organizationUrl, personalAccessToken);
```

### Add a General Comment

```csharp
var comment = await commentHelper.AddCommentAsync(
    projectName: "MyProject",
    repositoryId: "MyRepository", 
    pullRequestId: 123,
    commentText: "Great work on this feature!"
);
```

### Add a Line-Specific Comment

```csharp
var lineComment = await commentHelper.AddLineCommentAsync(
    projectName: "MyProject",
    repositoryId: "MyRepository",
    pullRequestId: 123,
    commentText: "Consider using a more descriptive variable name here.",
    filePath: "/src/Controllers/ApiController.cs",
    lineNumber: 45,
    isRightSide: true // true for new version, false for original
);
```

### Add Multiple Comments

```csharp
var comments = new List<PullRequestCommentRequest>
{
    new PullRequestCommentRequest
    {
        CommentText = "Please add unit tests for this method.",
        IsLineComment = true,
        FilePath = "/src/Services/UserService.cs",
        LineNumber = 25
    },
    new PullRequestCommentRequest
    {
        CommentText = "Overall looks good!"
    }
};

var createdThreads = await commentHelper.AddMultipleCommentsAsync(
    "MyProject", "MyRepository", 123, comments);
```

### Reply to a Comment

```csharp
await commentHelper.ReplyToCommentAsync(
    projectName: "MyProject",
    repositoryId: "MyRepository",
    pullRequestId: 123,
    threadId: commentThreadId,
    replyText: "Thanks for the feedback! I'll address this."
);
```

### Resolve a Comment Thread

```csharp
await commentHelper.UpdateCommentThreadStatusAsync(
    projectName: "MyProject",
    repositoryId: "MyRepository", 
    pullRequestId: 123,
    threadId: commentThreadId,
    status: CommentThreadStatus.Fixed
);
```

### Get All Comments

```csharp
var allThreads = await commentHelper.GetCommentThreadsAsync(
    "MyProject", "MyRepository", 123);

foreach (var thread in allThreads)
{
    Console.WriteLine($"Thread {thread.Id}: {thread.Comments?.FirstOrDefault()?.Content}");
}
```

### Export Comments to JSON

```csharp
await commentHelper.ExportCommentsToJsonAsync(
    allThreads, 
    @"C:\temp\pr-comments.json");
```

## Advanced Scenarios

### Automated Code Review

```csharp
public async Task AutomatedCodeReview(int pullRequestId)
{
    var reviewComments = new List<PullRequestCommentRequest>
    {
        new PullRequestCommentRequest
        {
            CommentText = "🔍 Consider extracting this logic into a separate method.",
            IsLineComment = true,
            FilePath = "/src/Controllers/UserController.cs",
            LineNumber = 32
        },
        new PullRequestCommentRequest
        {
            CommentText = "⚠️ This method should validate input parameters.",
            IsLineComment = true,
            FilePath = "/src/Services/AuthService.cs", 
            LineNumber = 67
        }
    };

    await commentHelper.AddMultipleCommentsAsync(
        "MyProject", "MyRepository", pullRequestId, reviewComments);
}
```

### Batch Comment Processing

```csharp
public async Task ProcessCodeReviewFeedback(IEnumerable<CodeReviewFinding> findings)
{
    var comments = findings.Select(finding => new PullRequestCommentRequest
    {
        CommentText = $"🔍 **{finding.Severity}**: {finding.Message}",
        IsLineComment = !string.IsNullOrEmpty(finding.FilePath),
        FilePath = finding.FilePath,
        LineNumber = finding.LineNumber
    });

    await commentHelper.AddMultipleCommentsAsync(
        projectName, repositoryId, pullRequestId, comments);
}
```

## Error Handling

The helper includes comprehensive error handling and validation:

```csharp
try 
{
    var comment = await commentHelper.AddCommentAsync(
        projectName, repositoryId, pullRequestId, commentText);
}
catch (ArgumentException ex)
{
    // Handle invalid parameters
    Console.WriteLine($"Invalid parameter: {ex.Message}");
}
catch (InvalidOperationException ex)
{
    // Handle Azure DevOps API errors
    Console.WriteLine($"Azure DevOps error: {ex.Message}");
}
```

## Configuration

### Repository ID

You can use either:
- Repository GUID: `"12345678-1234-1234-1234-123456789012"`
- Repository name: `"MyRepository"`

### Comment Thread Status Options

- `CommentThreadStatus.Active` - Active thread
- `CommentThreadStatus.Fixed` - Resolved/fixed
- `CommentThreadStatus.WontFix` - Won't fix
- `CommentThreadStatus.Closed` - Closed
- `CommentThreadStatus.ByDesign` - By design
- `CommentThreadStatus.Pending` - Pending

## Integration Examples

### GitHub Actions / Azure DevOps Pipelines

```yaml
- name: Add Code Review Comments
  run: |
    dotnet run --project CodeReviewTool -- \
      --organization "${{ vars.AZURE_DEVOPS_ORG }}" \
      --project "${{ vars.PROJECT_NAME }}" \
      --repository "${{ vars.REPOSITORY_NAME }}" \
      --pullrequest "${{ vars.PULL_REQUEST_ID }}" \
      --token "${{ secrets.AZURE_DEVOPS_PAT }}"
```

### Console Application

```csharp
class Program
{
    static async Task Main(string[] args)
    {
        var orgUrl = Environment.GetEnvironmentVariable("AZURE_DEVOPS_ORG_URL");
        var pat = Environment.GetEnvironmentVariable("AZURE_DEVOPS_PAT");
        
        using var commentHelper = new PullRequestCommentHelper(orgUrl, pat);
        
        // Add your comment logic here
        await commentHelper.AddCommentAsync(
            args[0], args[1], int.Parse(args[2]), args[3]);
    }
}
```

## Best Practices

1. **Always dispose the helper** - Use `using` statements or call `Dispose()`
2. **Validate inputs** - The helper validates parameters but additional validation is recommended
3. **Handle exceptions** - Wrap calls in try-catch blocks for production use
4. **Batch operations** - Use `AddMultipleCommentsAsync` for efficiency when adding many comments
5. **Use meaningful messages** - Include context and actionable feedback in comments
6. **Security** - Store PATs securely, never commit them to source control

## Dependencies

- Microsoft.TeamFoundationServer.Client (19.225.1+)
- Microsoft.VisualStudio.Services.Client (19.225.1+)
- Newtonsoft.Json (13.0.3+)
- .NET 8.0+

## License

This helper is part of the Common library in the code-review-skillset project.
