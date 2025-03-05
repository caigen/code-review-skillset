using Microsoft.AspNetCore.Mvc;

namespace GithubSkillsetSample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CodeReviewRulesController : ControllerBase
    {
        private static readonly CodeReviewRule[] CodeReviewRules = new[]
        {
            new CodeReviewRule()
            { 
                Id = "1", 
                Name = "Typo Check",
                Content = "Review if code or config has typo" 
            },

            new CodeReviewRule()
            {
                Id = "2",
                Name = "Code Format",
                Content = "Review if code is formatted properly"
            },

            new CodeReviewRule()
            {
                Id = "3",
                Name = "Code Structure",
                Content = "Review if code is structured properly"
            },

            new CodeReviewRule()
            {
                Id = "4",
                Name = "Code Duplication",
                Content = "Review if code is duplicated"
            },

            new CodeReviewRule()
            {
                Id = "5",
                Name = "Code Complexity",
                Content = "Review if code could be simplified"
            },

            new CodeReviewRule()
            {
                Id = "6",
                Name = "Code Comments",
                Content = "Review if code has comments"
            },

            new CodeReviewRule()
            {
                Id = "7",
                Name = "Code Test",
                Content = "Review if code has test"
            },

            new CodeReviewRule()
            {
                Id = "8",
                Name = "Null reference",
                Content = "Review if code has null reference protecction"
            },

            new CodeReviewRule()
            {
                Id = "9",
                Name = "Code Performance",
                Content = "Review if code is performant"
            },

            new CodeReviewRule()
            {
                Id = "10",
                Name = "Code Security",
                Content = "Review if code is secure"
            },

            new CodeReviewRule()
            {
                Id = "11",
                Name = "Code Resilience and Fallback",
                Content = "Review if code is handling fallback carefually"
            }
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public CodeReviewRulesController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("list")]
        public IEnumerable<CodeReviewRule> List()
        {
            return CodeReviewRules.ToArray();
        }

        [HttpGet]
        public IEnumerable<CodeReviewRule> Get(string? id)
        {
            if (id == null)
            {
                return CodeReviewRules;
            }

            return CodeReviewRules.Where(x => x.Id == id || x.Name == id).ToArray();
        }

        [HttpPost]
        public IEnumerable<CodeReviewRule> Post([FromBody] SkillsetRequest request)
        {
            Console.WriteLine("Post called");
            if (request.Id == null)
            {
                return CodeReviewRules;
            }

            return CodeReviewRules.Where(x => x.Id == request.Id || x.Name == request.Id).ToArray();
        }
    }
}
