using EmagineHackerNewsApi.DTOs;
using EmagineHackerNewsApi.Models;
using LazyCache;

namespace EmagineHackerNewsApi.Services
{
    public class StoriesService : IStoriesService
    {
        private readonly HttpClient _client;
        private readonly IAppCache _cache;

        public StoriesService(IHttpClientFactory clientFactory, IAppCache cache)
        {
            _client = clientFactory.CreateClient("HackerNewsClient");
            _cache = cache;
        }

        private async Task<Story?> GetStoryByIdAsync(int id, CancellationToken cancellationToken)
        {
            // Cache the list of stories for 30 minutes
            return await _cache.GetOrAddAsync($"story_{id}", async () =>
            {
                return await _client.GetFromJsonAsync<Story>($"item/{id}.json?print=pretty");
            }, TimeSpan.FromMinutes(30));
        }

        public async Task<List<StoryDto>> GetBestStoriesAsync(int qtdStories, CancellationToken cancellationToken = default)
        {
            // Cache the list of IDs for 30 seconds
            var ids = await _cache.GetOrAddAsync("beststories_ids", async () =>
            {
                return await _client.GetFromJsonAsync<List<int>>("beststories.json?print=pretty", cancellationToken);
            }, TimeSpan.FromSeconds(30));

            if (ids == null || ids.Count == 0)
            {
                return new List<StoryDto>();
            }

            // Fetch more stories than requested to ensure enough valid ones are available
            var tasks = ids.Take(qtdStories * 2).Select(id => GetStoryByIdAsync(id, cancellationToken));
            var stories = await Task.WhenAll(tasks);

            // Filter, sort, and transform the stories into DTOs
            return stories
            .Where(s => s != null)
            .OrderByDescending(s => s.Score)
            .Take(qtdStories)
            .Select(s => new StoryDto
            {
                Title = s.Title,
                Uri = s.Url,
                PostedBy = s.By,
                Time = DateTimeOffset.FromUnixTimeSeconds(s.Time).UtcDateTime.ToString("o"),
                Score = s.Score,
                CommentCount = s.Descendants
            })
            .ToList();
        }
    }
}

