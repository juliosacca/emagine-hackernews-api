using EmagineHackerNewsApi.DTOs;
using EmagineHackerNewsApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmagineHackerNewsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BestStoriesController : ControllerBase
    {
        private readonly IStoriesService _storiesService;     
        public BestStoriesController(IStoriesService storiesService)
        {
            _storiesService = storiesService;
        }

        /// <summary>
        /// Retrieves the top N best Hacker News stories, ordered by score.
        /// </summary>
        /// <param name="n">The number of stories to return (1–500).</param>
        /// <returns>A list of stories with title, URI, author, time, score, and comment count.</returns>
        /// <response code="200">Returns the list of stories</response>
        /// <response code="400">If n is invalid</response>
        /// <response code="404">If no stories are found</response>

        [HttpGet("{n}")]
        public async Task<ActionResult<List<StoryDto>>> GetBestStoriesAsync(int n)
        {
            if (n <= 0)
            {
                return BadRequest("number of the top n stories must be greater than zero.");
            }

            if (n > 500)
                return BadRequest("number of the top n stories cannot be greater than 500.");

            var stories = await _storiesService.GetBestStoriesAsync(n);

            var validStories = stories?.Where(s => s != null).ToList() ?? new List<StoryDto>();
            if (validStories.Count == 0)
                return NotFound("No stories found.");

            return Ok(validStories);
        }

    }
}
