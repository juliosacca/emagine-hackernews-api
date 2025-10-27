using EmagineHackerNewsApi.DTOs;
using EmagineHackerNewsApi.Models;

namespace EmagineHackerNewsApi.Services;

public interface IStoriesService
{
    Task<List<StoryDto>> GetBestStoriesAsync(int qtdStories, CancellationToken cancellationToken = default);
}