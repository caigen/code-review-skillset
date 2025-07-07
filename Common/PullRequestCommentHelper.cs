using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json;
using System.Text;

namespace Common
{
    public class PullRequestCommentHelper : IDisposable
    {
        private readonly string _organizationUrl;
        private readonly string _personalAccessToken;
        private readonly VssConnection _connection;

        public PullRequestCommentHelper(string organizationUrl, string personalAccessToken)
        {
            _organizationUrl = organizationUrl ?? throw new ArgumentNullException(nameof(organizationUrl));
            _personalAccessToken = personalAccessToken ?? throw new ArgumentNullException(nameof(personalAccessToken));
            
            var credentials = new VssBasicCredential(string.Empty, _personalAccessToken);
            _connection = new VssConnection(new Uri(_organizationUrl), credentials);
        }

        /// <summary>
        /// Adds a comment to a pull request
        /// </summary>
        /// <param name="projectName">The name of the Azure DevOps project</param>
        /// <param name="repositoryId">The ID or name of the repository</param>
        /// <param name="pullRequestId">The ID of the pull request</param>
        /// <param name="commentText">The comment text to add</param>
        /// <param name="parentCommentId">Optional parent comment ID for replies</param>
        /// <returns>The created comment</returns>
        public async Task<GitPullRequestCommentThread> AddCommentAsync(
            string projectName, 
            string repositoryId, 
            int pullRequestId, 
            string commentText,
            int? parentCommentId = null)
        {
            if (string.IsNullOrWhiteSpace(projectName))
                throw new ArgumentException("Project name cannot be null or empty", nameof(projectName));

            if (string.IsNullOrWhiteSpace(repositoryId))
                throw new ArgumentException("Repository ID cannot be null or empty", nameof(repositoryId));

            if (pullRequestId <= 0)
                throw new ArgumentException("Pull request ID must be greater than 0", nameof(pullRequestId));

            if (string.IsNullOrWhiteSpace(commentText))
                throw new ArgumentException("Comment text cannot be null or empty", nameof(commentText));

            try
            {
                using var gitClient = _connection.GetClient<GitHttpClient>();

                var comment = new Comment
                {
                    Content = commentText,
                    CommentType = CommentType.Text,
                    ParentCommentId = parentCommentId
                };

                var thread = new GitPullRequestCommentThread
                {
                    Comments = new List<Comment> { comment },
                    Status = CommentThreadStatus.Active
                };

                var createdThread = await gitClient.CreateThreadAsync(
                    thread, 
                    repositoryId, 
                    pullRequestId, 
                    project: projectName);

                return createdThread;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to add comment to pull request {pullRequestId} in repository {repositoryId}", ex);
            }
        }

        /// <summary>
        /// Adds a comment to a specific line in a pull request file
        /// </summary>
        /// <param name="projectName">The name of the Azure DevOps project</param>
        /// <param name="repositoryId">The ID or name of the repository</param>
        /// <param name="pullRequestId">The ID of the pull request</param>
        /// <param name="commentText">The comment text to add</param>
        /// <param name="filePath">The path to the file being commented on</param>
        /// <param name="lineNumber">The line number to comment on</param>
        /// <param name="isRightSide">True for new file version, false for original version</param>
        /// <returns>The created comment thread</returns>
        public async Task<GitPullRequestCommentThread> AddLineCommentAsync(
            string projectName,
            string repositoryId,
            int pullRequestId,
            string commentText,
            string filePath,
            int lineNumber,
            bool isRightSide = true)
        {
            if (string.IsNullOrWhiteSpace(projectName))
                throw new ArgumentException("Project name cannot be null or empty", nameof(projectName));

            if (string.IsNullOrWhiteSpace(repositoryId))
                throw new ArgumentException("Repository ID cannot be null or empty", nameof(repositoryId));

            if (pullRequestId <= 0)
                throw new ArgumentException("Pull request ID must be greater than 0", nameof(pullRequestId));

            if (string.IsNullOrWhiteSpace(commentText))
                throw new ArgumentException("Comment text cannot be null or empty", nameof(commentText));

            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

            if (lineNumber <= 0)
                throw new ArgumentException("Line number must be greater than 0", nameof(lineNumber));

            try
            {
                using var gitClient = _connection.GetClient<GitHttpClient>();

                var comment = new Comment
                {
                    Content = commentText,
                    CommentType = CommentType.Text
                };

                var thread = new GitPullRequestCommentThread
                {
                    Comments = new List<Comment> { comment },
                    Status = CommentThreadStatus.Active,
                    ThreadContext = new CommentThreadContext
                    {
                        FilePath = filePath,
                        RightFileStart = isRightSide ? new CommentPosition { Line = lineNumber, Offset = 1 } : null,
                        LeftFileStart = !isRightSide ? new CommentPosition { Line = lineNumber, Offset = 1 } : null
                    }
                };

                var createdThread = await gitClient.CreateThreadAsync(
                    thread,
                    repositoryId,
                    pullRequestId,
                    project: projectName);

                return createdThread;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to add line comment to pull request {pullRequestId} at line {lineNumber} in file {filePath}", ex);
            }
        }

        /// <summary>
        /// Adds multiple comments to a pull request
        /// </summary>
        /// <param name="projectName">The name of the Azure DevOps project</param>
        /// <param name="repositoryId">The ID or name of the repository</param>
        /// <param name="pullRequestId">The ID of the pull request</param>
        /// <param name="comments">Collection of comments to add</param>
        /// <returns>Collection of created comment threads</returns>
        public async Task<IEnumerable<GitPullRequestCommentThread>> AddMultipleCommentsAsync(
            string projectName,
            string repositoryId,
            int pullRequestId,
            IEnumerable<PullRequestCommentRequest> comments)
        {
            if (string.IsNullOrWhiteSpace(projectName))
                throw new ArgumentException("Project name cannot be null or empty", nameof(projectName));

            if (string.IsNullOrWhiteSpace(repositoryId))
                throw new ArgumentException("Repository ID cannot be null or empty", nameof(repositoryId));

            if (pullRequestId <= 0)
                throw new ArgumentException("Pull request ID must be greater than 0", nameof(pullRequestId));

            if (comments == null || !comments.Any())
                throw new ArgumentException("Comments collection cannot be null or empty", nameof(comments));

            var createdThreads = new List<GitPullRequestCommentThread>();

            foreach (var commentRequest in comments)
            {
                try
                {
                    GitPullRequestCommentThread thread;

                    if (commentRequest.IsLineComment && !string.IsNullOrWhiteSpace(commentRequest.FilePath) && commentRequest.LineNumber.HasValue)
                    {
                        thread = await AddLineCommentAsync(
                            projectName,
                            repositoryId,
                            pullRequestId,
                            commentRequest.CommentText,
                            commentRequest.FilePath,
                            commentRequest.LineNumber.Value,
                            commentRequest.IsRightSide);
                    }
                    else
                    {
                        thread = await AddCommentAsync(
                            projectName,
                            repositoryId,
                            pullRequestId,
                            commentRequest.CommentText,
                            commentRequest.ParentCommentId);
                    }

                    createdThreads.Add(thread);
                }
                catch (Exception ex)
                {
                    // Log the error but continue with other comments
                    Console.WriteLine($"Failed to add comment '{commentRequest.CommentText}': {ex.Message}");
                }
            }

            return createdThreads;
        }

        /// <summary>
        /// Updates the status of a comment thread (e.g., resolve, reactivate)
        /// </summary>
        /// <param name="projectName">The name of the Azure DevOps project</param>
        /// <param name="repositoryId">The ID or name of the repository</param>
        /// <param name="pullRequestId">The ID of the pull request</param>
        /// <param name="threadId">The ID of the comment thread</param>
        /// <param name="status">The new status for the thread</param>
        /// <returns>The updated comment thread</returns>
        public async Task<GitPullRequestCommentThread> UpdateCommentThreadStatusAsync(
            string projectName,
            string repositoryId,
            int pullRequestId,
            int threadId,
            CommentThreadStatus status)
        {
            if (string.IsNullOrWhiteSpace(projectName))
                throw new ArgumentException("Project name cannot be null or empty", nameof(projectName));

            if (string.IsNullOrWhiteSpace(repositoryId))
                throw new ArgumentException("Repository ID cannot be null or empty", nameof(repositoryId));

            if (pullRequestId <= 0)
                throw new ArgumentException("Pull request ID must be greater than 0", nameof(pullRequestId));

            if (threadId <= 0)
                throw new ArgumentException("Thread ID must be greater than 0", nameof(threadId));

            try
            {
                using var gitClient = _connection.GetClient<GitHttpClient>();

                var updateThread = new GitPullRequestCommentThread
                {
                    Status = status
                };

                var updatedThread = await gitClient.UpdateThreadAsync(
                    updateThread,
                    repositoryId,
                    pullRequestId,
                    threadId,
                    project: projectName);

                return updatedThread;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to update comment thread {threadId} status in pull request {pullRequestId}", ex);
            }
        }

        /// <summary>
        /// Gets all comment threads for a pull request
        /// </summary>
        /// <param name="projectName">The name of the Azure DevOps project</param>
        /// <param name="repositoryId">The ID or name of the repository</param>
        /// <param name="pullRequestId">The ID of the pull request</param>
        /// <returns>Collection of comment threads</returns>
        public async Task<IEnumerable<GitPullRequestCommentThread>> GetCommentThreadsAsync(
            string projectName,
            string repositoryId,
            int pullRequestId)
        {
            if (string.IsNullOrWhiteSpace(projectName))
                throw new ArgumentException("Project name cannot be null or empty", nameof(projectName));

            if (string.IsNullOrWhiteSpace(repositoryId))
                throw new ArgumentException("Repository ID cannot be null or empty", nameof(repositoryId));

            if (pullRequestId <= 0)
                throw new ArgumentException("Pull request ID must be greater than 0", nameof(pullRequestId));

            try
            {
                using var gitClient = _connection.GetClient<GitHttpClient>();

                var threads = await gitClient.GetThreadsAsync(
                    repositoryId,
                    pullRequestId,
                    project: projectName);

                return threads;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to retrieve comment threads for pull request {pullRequestId} in repository {repositoryId}", ex);
            }
        }

        /// <summary>
        /// Replies to an existing comment thread
        /// </summary>
        /// <param name="projectName">The name of the Azure DevOps project</param>
        /// <param name="repositoryId">The ID or name of the repository</param>
        /// <param name="pullRequestId">The ID of the pull request</param>
        /// <param name="threadId">The ID of the comment thread to reply to</param>
        /// <param name="replyText">The reply text</param>
        /// <returns>The updated comment thread</returns>
        public async Task<GitPullRequestCommentThread> ReplyToCommentAsync(
            string projectName,
            string repositoryId,
            int pullRequestId,
            int threadId,
            string replyText)
        {
            if (string.IsNullOrWhiteSpace(projectName))
                throw new ArgumentException("Project name cannot be null or empty", nameof(projectName));

            if (string.IsNullOrWhiteSpace(repositoryId))
                throw new ArgumentException("Repository ID cannot be null or empty", nameof(repositoryId));

            if (pullRequestId <= 0)
                throw new ArgumentException("Pull request ID must be greater than 0", nameof(pullRequestId));

            if (threadId <= 0)
                throw new ArgumentException("Thread ID must be greater than 0", nameof(threadId));

            if (string.IsNullOrWhiteSpace(replyText))
                throw new ArgumentException("Reply text cannot be null or empty", nameof(replyText));

            try
            {
                using var gitClient = _connection.GetClient<GitHttpClient>();

                var reply = new Comment
                {
                    Content = replyText,
                    CommentType = CommentType.Text
                };

                var updatedThread = await gitClient.CreateCommentAsync(
                    reply,
                    repositoryId,
                    pullRequestId,
                    threadId,
                    project: projectName);

                return updatedThread;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to reply to comment thread {threadId} in pull request {pullRequestId}", ex);
            }
        }

        /// <summary>
        /// Exports pull request comments to JSON format
        /// </summary>
        /// <param name="comments">The comment threads to export</param>
        /// <param name="filePath">Path to save the JSON file</param>
        public async Task ExportCommentsToJsonAsync(IEnumerable<GitPullRequestCommentThread> comments, string filePath)
        {
            if (comments == null)
                throw new ArgumentNullException(nameof(comments));

            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonConvert.SerializeObject(comments, Formatting.Indented);
            await File.WriteAllTextAsync(filePath, json, Encoding.UTF8);
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }

    /// <summary>
    /// Represents a pull request comment request
    /// </summary>
    public class PullRequestCommentRequest
    {
        public string CommentText { get; set; } = string.Empty;
        public bool IsLineComment { get; set; }
        public string? FilePath { get; set; }
        public int? LineNumber { get; set; }
        public bool IsRightSide { get; set; } = true;
        public int? ParentCommentId { get; set; }
    }

    /// <summary>
    /// Represents a simplified comment thread summary
    /// </summary>
    public class CommentThreadSummary
    {
        public int ThreadId { get; set; }
        public string? Status { get; set; }
        public string? FilePath { get; set; }
        public int? LineNumber { get; set; }
        public int CommentCount { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public string? Author { get; set; }
        public string? FirstCommentText { get; set; }
    }
}
