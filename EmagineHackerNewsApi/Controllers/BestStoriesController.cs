using EmagineHackerNewsApi.Models;
using EmagineHackerNewsApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace EmagineHackerNewsApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BestStoriesController : ControllerBase
    {
        private readonly IStoriesService _storiesService;
        
        public BestStoriesController(IStoriesService storiesService)
        {
            _storiesService = storiesService;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetBestStoriesAsync(int qtdStories)
        {
            var response = await _storiesService.GetBestStoriesAsync(qtdStories);
            return Ok(response);
        }
        
    }
}
