using Common;

namespace Common.Examples
{
    /// <summary>
    /// Example usage of PullRequestCommentHelper
    /// </summary>
    public class PullRequestCommentHelperExample
    {
        private readonly string _organizationUrl = "https://dev.azure.com/your-organization";
        private readonly string _personalAccessToken = "your-pat-token";

        public async Task DemonstrateUsageAsync()
        {
            using var commentHelper = new PullRequestCommentHelper(_organizationUrl, _personalAccessToken);

            var projectName = "MyProject";
            var repositoryId = "MyRepository";
            var pullRequestId = 123;

            try
            {
                // Example 1: Add a general comment to a pull request
                var generalComment = await commentHelper.AddCommentAsync(
                    projectName,
                    repositoryId,
                    pullRequestId,
                    "This looks good! Great work on implementing the new feature.");

                Console.WriteLine($"Added general comment with thread ID: {generalComment.Id}");

                // Example 2: Add a comment to a specific line in a file
                var lineComment = await commentHelper.AddLineCommentAsync(
                    projectName,
                    repositoryId,
                    pullRequestId,
                    "Consider using a more descriptive variable name here.",
                    "/src/Controllers/ApiController.cs",
                    45,
                    isRightSide: true);

                Console.WriteLine($"Added line comment with thread ID: {lineComment.Id}");

                // Example 3: Add multiple comments at once
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
                        CommentText = "Consider adding error handling here.",
                        IsLineComment = true,
                        FilePath = "/src/Services/UserService.cs",
                        LineNumber = 50
                    },
                    new PullRequestCommentRequest
                    {
                        CommentText = "Overall, this PR implements the requirements well. Just a few minor suggestions above."
                    }
                };

                var createdThreads = await commentHelper.AddMultipleCommentsAsync(
                    projectName,
                    repositoryId,
                    pullRequestId,
                    comments);

                Console.WriteLine($"Added {createdThreads.Count()} comments to the pull request");

                // Example 4: Reply to an existing comment
                if (generalComment.Id.HasValue)
                {
                    await commentHelper.ReplyToCommentAsync(
                        projectName,
                        repositoryId,
                        pullRequestId,
                        generalComment.Id.Value,
                        "Thanks for the feedback! I'll address these suggestions.");
                }

                // Example 5: Get all comment threads for the pull request
                var allThreads = await commentHelper.GetCommentThreadsAsync(
                    projectName,
                    repositoryId,
                    pullRequestId);

                Console.WriteLine($"Pull request has {allThreads.Count()} comment threads");

                // Example 6: Resolve a comment thread
                if (lineComment.Id.HasValue)
                {
                    await commentHelper.UpdateCommentThreadStatusAsync(
                        projectName,
                        repositoryId,
                        pullRequestId,
                        lineComment.Id.Value,
                        Microsoft.TeamFoundation.SourceControl.WebApi.CommentThreadStatus.Fixed);

                    Console.WriteLine($"Resolved comment thread {lineComment.Id}");
                }

                // Example 7: Export comments to JSON file
                await commentHelper.ExportCommentsToJsonAsync(
                    allThreads,
                    @"C:\temp\pr-comments.json");

                Console.WriteLine("Exported comments to JSON file");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Example of automated code review comments based on common patterns
        /// </summary>
        public async Task AutomatedCodeReviewExample()
        {
            using var commentHelper = new PullRequestCommentHelper(_organizationUrl, _personalAccessToken);

            var projectName = "MyProject";
            var repositoryId = "MyRepository";
            var pullRequestId = 123;

            // Common code review suggestions
            var codeReviewComments = new List<PullRequestCommentRequest>
            {
                new PullRequestCommentRequest
                {
                    CommentText = "🔍 **Code Review Suggestion**: Consider extracting this logic into a separate method for better readability.",
                    IsLineComment = true,
                    FilePath = "/src/Controllers/UserController.cs",
                    LineNumber = 32
                },
                new PullRequestCommentRequest
                {
                    CommentText = "⚠️ **Security Concern**: This method should validate input parameters to prevent potential security vulnerabilities.",
                    IsLineComment = true,
                    FilePath = "/src/Services/AuthService.cs",
                    LineNumber = 67
                },
                new PullRequestCommentRequest
                {
                    CommentText = "📝 **Documentation**: Please add XML documentation comments for this public method.",
                    IsLineComment = true,
                    FilePath = "/src/Models/User.cs",
                    LineNumber = 15
                },
                new PullRequestCommentRequest
                {
                    CommentText = "🧪 **Testing**: This new functionality would benefit from unit tests. Consider adding tests in the corresponding test project.",
                    IsLineComment = true,
                    FilePath = "/src/Services/NotificationService.cs",
                    LineNumber = 89
                },
                new PullRequestCommentRequest
                {
                    CommentText = "✅ **Automated Code Review Complete**: I've reviewed the changes and added suggestions above. Overall, the implementation looks solid!"
                }
            };

            try
            {
                var createdThreads = await commentHelper.AddMultipleCommentsAsync(
                    projectName,
                    repositoryId,
                    pullRequestId,
                    codeReviewComments);

                Console.WriteLine($"Automated code review completed. Added {createdThreads.Count()} review comments.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Automated code review failed: {ex.Message}");
            }
        }
    }
}
