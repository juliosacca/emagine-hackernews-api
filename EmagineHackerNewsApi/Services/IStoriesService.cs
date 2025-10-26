using EmagineHackerNewsApi.Models;

namespace EmagineHackerNewsApi.Services;

public interface IStoriesService
{
    Task<ResponseId> GetBestStoriesAsync(int qtdStories, CancellationToken cancellationToken = default);
}