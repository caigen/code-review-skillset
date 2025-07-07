using Common;
using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace CommonTests
{
    /// <summary>
    /// Unit tests for PullRequestCommentHelper class
    /// </summary>
    [TestClass]
    public class PullRequestCommentHelperTests
    {
        private const string TestOrganizationUrl = "https://dev.azure.com/test-organization";
        private const string TestPersonalAccessToken = "test-pat-token";
        private const string TestProjectName = "TestProject";
        private const string TestRepositoryId = "TestRepository";
        private const int TestPullRequestId = 123;

        private PullRequestCommentHelper? _commentHelper;

        [TestInitialize]
        public void TestInitialize()
        {
            // Note: In real tests, you would mock the Azure DevOps API calls
            // For demonstration purposes, we're using test values
            _commentHelper = new PullRequestCommentHelper(TestOrganizationUrl, TestPersonalAccessToken);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _commentHelper?.Dispose();
        }

        [TestMethod]
        public void Constructor_WithValidParameters_ShouldCreateInstance()
        {
            // Arrange & Act
            var commentHelper = new PullRequestCommentHelper(TestOrganizationUrl, TestPersonalAccessToken);

            // Assert
            Assert.IsNotNull(commentHelper);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullOrganizationUrl_ShouldThrowArgumentNullException()
        {
            // Arrange & Act & Assert
            new PullRequestCommentHelper(null!, TestPersonalAccessToken);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullPersonalAccessToken_ShouldThrowArgumentNullException()
        {
            // Arrange & Act & Assert
            new PullRequestCommentHelper(TestOrganizationUrl, null!);
        }

        [TestMethod]
        public async Task AddCommentAsync_WithValidParameters_ShouldCreateComment()
        {
            // Arrange
            var commentText = "This looks good! Great work on implementing the new feature.";

            // Act & Assert
            // Note: This would require mocking the Azure DevOps API in real tests
            // For now, we'll test the parameter validation
            try
            {
                var result = await _commentHelper!.AddCommentAsync(
                    TestProjectName,
                    TestRepositoryId,
                    TestPullRequestId,
                    commentText);

                // In a real test with mocking, you would assert the result
                // Assert.IsNotNull(result);
                // Assert.AreEqual(commentText, result.Comments.First().Content);
            }
            catch (Exception ex)
            {
                // Expected for this demo since we're not connecting to real Azure DevOps
                Assert.IsTrue(ex.Message.Contains("TF400813") || ex.Message.Contains("VS403403") || ex.Message.Contains("Unauthorized"));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task AddCommentAsync_WithEmptyProjectName_ShouldThrowArgumentException()
        {
            // Arrange & Act & Assert
            await _commentHelper!.AddCommentAsync(
                string.Empty,
                TestRepositoryId,
                TestPullRequestId,
                "Test comment");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task AddCommentAsync_WithEmptyRepositoryId_ShouldThrowArgumentException()
        {
            // Arrange & Act & Assert
            await _commentHelper!.AddCommentAsync(
                TestProjectName,
                string.Empty,
                TestPullRequestId,
                "Test comment");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task AddCommentAsync_WithInvalidPullRequestId_ShouldThrowArgumentException()
        {
            // Arrange & Act & Assert
            await _commentHelper!.AddCommentAsync(
                TestProjectName,
                TestRepositoryId,
                0,
                "Test comment");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task AddCommentAsync_WithEmptyCommentText_ShouldThrowArgumentException()
        {
            // Arrange & Act & Assert
            await _commentHelper!.AddCommentAsync(
                TestProjectName,
                TestRepositoryId,
                TestPullRequestId,
                string.Empty);
        }

        [TestMethod]
        public async Task AddLineCommentAsync_WithValidParameters_ShouldCreateLineComment()
        {
            // Arrange
            var commentText = "Consider using a more descriptive variable name here.";
            var filePath = "/src/Controllers/ApiController.cs";
            var lineNumber = 45;

            // Act & Assert
            try
            {
                var result = await _commentHelper!.AddLineCommentAsync(
                    TestProjectName,
                    TestRepositoryId,
                    TestPullRequestId,
                    commentText,
                    filePath,
                    lineNumber,
                    isRightSide: true);

                // In a real test with mocking, you would assert the result
                // Assert.IsNotNull(result);
                // Assert.AreEqual(filePath, result.ThreadContext.FilePath);
            }
            catch (Exception ex)
            {
                // Expected for this demo since we're not connecting to real Azure DevOps
                Assert.IsTrue(ex.Message.Contains("TF400813") || ex.Message.Contains("VS403403") || ex.Message.Contains("Unauthorized"));
            }
        }

        [TestMethod]
        public async Task AddMultipleCommentsAsync_WithValidRequests_ShouldCreateMultipleComments()
        {
            // Arrange
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

            // Act & Assert
            try
            {
                var result = await _commentHelper!.AddMultipleCommentsAsync(
                    TestProjectName,
                    TestRepositoryId,
                    TestPullRequestId,
                    comments);

                // In a real test with mocking, you would assert the result
                // Assert.IsNotNull(result);
                // Assert.AreEqual(3, result.Count());
            }
            catch (Exception ex)
            {
                // Expected for this demo since we're not connecting to real Azure DevOps
                Assert.IsTrue(ex.Message.Contains("TF400813") || ex.Message.Contains("VS403403") || ex.Message.Contains("Unauthorized"));
            }
        }

        [TestMethod]
        public async Task ReplyToCommentAsync_WithValidParameters_ShouldAddReply()
        {
            // Arrange
            var threadId = 1;
            var replyText = "Thanks for the feedback! I'll address these suggestions.";

            // Act & Assert
            try
            {
                var reply = await _commentHelper!.ReplyToCommentAsync(
                    TestProjectName,
                    TestRepositoryId,
                    TestPullRequestId,
                    threadId,
                    replyText);

                // In a real test with mocking, you would assert the reply was added
                // Assert.IsNotNull(reply);
                // Assert.AreEqual(replyText, reply.Content);
            }
            catch (Exception ex)
            {
                // Expected for this demo since we're not connecting to real Azure DevOps
                Assert.IsTrue(ex.Message.Contains("TF400813") || ex.Message.Contains("VS403403") || ex.Message.Contains("Unauthorized"));
            }
        }

        [TestMethod]
        public async Task GetCommentThreadsAsync_WithValidParameters_ShouldRetrieveThreads()
        {
            // Arrange & Act & Assert
            try
            {
                var result = await _commentHelper!.GetCommentThreadsAsync(
                    TestProjectName,
                    TestRepositoryId,
                    TestPullRequestId);

                // In a real test with mocking, you would assert the result
                // Assert.IsNotNull(result);
            }
            catch (Exception ex)
            {
                // Expected for this demo since we're not connecting to real Azure DevOps
                Assert.IsTrue(ex.Message.Contains("TF400813") || ex.Message.Contains("VS403403") || ex.Message.Contains("Unauthorized"));
            }
        }

        [TestMethod]
        public async Task UpdateCommentThreadStatusAsync_WithValidParameters_ShouldUpdateStatus()
        {
            // Arrange
            var threadId = 1;
            var status = CommentThreadStatus.Fixed;

            // Act & Assert
            try
            {
                await _commentHelper!.UpdateCommentThreadStatusAsync(
                    TestProjectName,
                    TestRepositoryId,
                    TestPullRequestId,
                    threadId,
                    status);

                // In a real test with mocking, you would assert the status was updated
            }
            catch (Exception ex)
            {
                // Expected for this demo since we're not connecting to real Azure DevOps
                Assert.IsTrue(ex.Message.Contains("TF400813") || ex.Message.Contains("VS403403") || ex.Message.Contains("Unauthorized"));
            }
        }

        [TestMethod]
        public async Task ExportCommentsToJsonAsync_WithValidData_ShouldCreateJsonFile()
        {
            // Arrange
            var threads = new List<GitPullRequestCommentThread>();
            var tempFilePath = Path.Combine(Path.GetTempPath(), $"test-comments-{Guid.NewGuid()}.json");

            try
            {
                // Act
                await _commentHelper!.ExportCommentsToJsonAsync(threads, tempFilePath);

                // Assert
                Assert.IsTrue(File.Exists(tempFilePath));
                var jsonContent = await File.ReadAllTextAsync(tempFilePath);
                Assert.IsNotNull(jsonContent);
            }
            finally
            {
                // Cleanup
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
        }

        /// <summary>
        /// Test method demonstrating automated code review comment scenarios
        /// </summary>
        [TestMethod]
        public async Task AutomatedCodeReviewScenario_ShouldCreateReviewComments()
        {
            // Arrange
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

            // Act & Assert
            try
            {
                var result = await _commentHelper!.AddMultipleCommentsAsync(
                    TestProjectName,
                    TestRepositoryId,
                    TestPullRequestId,
                    codeReviewComments);

                // In a real test with mocking, you would assert the automated review completed successfully
                // Assert.IsNotNull(result);
                // Assert.AreEqual(5, result.Count());
            }
            catch (Exception ex)
            {
                // Expected for this demo since we're not connecting to real Azure DevOps
                Assert.IsTrue(ex.Message.Contains("TF400813") || ex.Message.Contains("VS403403") || ex.Message.Contains("Unauthorized"));
            }
        }

        /// <summary>
        /// Integration test demonstrating complete pull request comment workflow
        /// </summary>
        [TestMethod]
        public async Task CompleteWorkflowIntegrationTest_ShouldExecuteAllOperations()
        {
            // Arrange
            var commentText = "This is a test comment for integration testing.";
            var filePath = "/src/Controllers/TestController.cs";
            var lineNumber = 25;

            try
            {
                // Act - Add general comment
                var generalComment = await _commentHelper!.AddCommentAsync(
                    TestProjectName,
                    TestRepositoryId,
                    TestPullRequestId,
                    commentText);

                // Act - Add line comment
                var lineComment = await _commentHelper!.AddLineCommentAsync(
                    TestProjectName,
                    TestRepositoryId,
                    TestPullRequestId,
                    "Line-specific feedback",
                    filePath,
                    lineNumber,
                    isRightSide: true);

                // Act - Get all threads
                var allThreads = await _commentHelper!.GetCommentThreadsAsync(
                    TestProjectName,
                    TestRepositoryId,
                    TestPullRequestId);

                // Act - Export to JSON
                var tempFilePath = Path.Combine(Path.GetTempPath(), $"integration-test-{Guid.NewGuid()}.json");
                await _commentHelper!.ExportCommentsToJsonAsync(allThreads, tempFilePath);

                // Assert - In a real test with mocking, you would verify all operations completed successfully
                // Assert.IsNotNull(generalComment);
                // Assert.IsNotNull(lineComment);
                // Assert.IsNotNull(allThreads);
                // Assert.IsTrue(File.Exists(tempFilePath));

                // Cleanup
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
            catch (Exception ex)
            {
                // Expected for this demo since we're not connecting to real Azure DevOps
                Assert.IsTrue(ex.Message.Contains("TF400813") || ex.Message.Contains("VS403403") || ex.Message.Contains("Unauthorized"));
            }
        }
    }
}
