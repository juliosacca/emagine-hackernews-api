using EmagineHackerNewsApi.Models;

namespace EmagineHackerNewsApi.Services
{
    public class StoriesService : IStoriesService
    {
        private readonly HttpClient _client;

        public StoriesService(IHttpClientFactory clientFactory)
        {
            _client = clientFactory.CreateClient("HackerNewsClient");
        }

        public async Task<ResponseId> GetBestStoriesAsync(int qtdStories, CancellationToken cancellationToken = default)
        {
            var ids = await _client.GetFromJsonAsync<List<int>>($"beststories.json?print=pretty", cancellationToken);

            if (ids == null || ids.Count == 0)
            {
                return new ResponseId();
            }

            //var tasks = ids.Select(async id => await GetStoryByIdAsync(id));

            //var stories = (await Task.WhenAll(tasks)).Where(s => s != null).Cast<Story>().ToList();

            // var filteredStories = string.IsNullOrEmpty(query)
            //     ? stories
            //     : stories.Where(s => !string.IsNullOrEmpty(s.Title) && s.Title.Contains(query, StringComparison.OrdinalIgnoreCase));

            //var paginatedStories = filteredStories.Skip((page - 1) * pageSize).Take(pageSize);
            return new ResponseId()
            {
                Total = ids.Count(),
                //Page = page,
                //PageSize = pageSize,
                // Stories = paginatedStories
                Id = ids
            };

        }
        
    }

}

