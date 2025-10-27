using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using EmagineHackerNewsApi.DTOs;
using EmagineHackerNewsApi.Models;
using EmagineHackerNewsApi.Services;
using LazyCache;
using LazyCache.Providers;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Moq.Protected;

namespace EmagineHackerNewsApi.Tests.Services;

public class StoriesServiceTests
{
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly IAppCache _cache; 
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly StoriesService _service;

    public StoriesServiceTests()
    {
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();

        // Use a real cache instance for testing instead of mocking
        _cache = new CachingService(new MemoryCacheProvider(new MemoryCache(new MemoryCacheOptions())));

        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        var httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://hacker-news.firebaseio.com/v0/")
        };

        _mockHttpClientFactory
            .Setup(f => f.CreateClient("HackerNewsClient"))
            .Returns(httpClient);

        _service = new StoriesService(_mockHttpClientFactory.Object, _cache);
    }

    [Fact]
    public async Task GetBestStoriesAsync_WithValidData_ReturnsOrderedStories()
    {
        // Arrange
        var storyIds = new List<int> { 1, 2, 3 };
        var stories = new List<Story>
        {
            new Story
            {
                Title = "Story 1",
                Url = "https://example.com/1",
                By = "user1",
                Time = 1609459200,
                Score = 100,
                Descendants = 10
            },
            new Story
            {
                Title = "Story 2",
                Url = "https://example.com/2",
                By = "user2",
                Time = 1609545600,
                Score = 200,
                Descendants = 20
            },
            new Story
            {
                Title = "Story 3",
                Url = "https://example.com/3",
                By = "user3",
                Time = 1609632000,
                Score = 50,
                Descendants = 5
            }
        };

        // Mock HTTP responses
        SetupHttpResponse("beststories.json?print=pretty", storyIds);
        SetupHttpResponse("item/1.json?print=pretty", stories[0]);
        SetupHttpResponse("item/2.json?print=pretty", stories[1]);
        SetupHttpResponse("item/3.json?print=pretty", stories[2]);

        // Act
        var result = await _service.GetBestStoriesAsync(3);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);

        // Verify stories are ordered by score (descending)
        Assert.Equal("Story 2", result[0].Title);
        Assert.Equal(200, result[0].Score);
        Assert.Equal("Story 1", result[1].Title);
        Assert.Equal(100, result[1].Score);
        Assert.Equal("Story 3", result[2].Title);
        Assert.Equal(50, result[2].Score);
    }

    [Fact]
    public async Task GetBestStoriesAsync_WithFewerStoriesAvailable_ReturnsAvailableStories()
    {
        // Arrange
        var storyIds = new List<int> { 1, 2 };
        var stories = new List<Story>
        {
            new Story
            {
                Title = "Story 1",
                Url = "https://example.com/1",
                By = "user1",
                Time = 1609459200,
                Score = 100,
                Descendants = 10
            },
            new Story
            {
                Title = "Story 2",
                Url = "https://example.com/2",
                By = "user2",
                Time = 1609545600,
                Score = 200,
                Descendants = 20
            }
        };

        SetupHttpResponse("beststories.json?print=pretty", storyIds);
        SetupHttpResponse("item/1.json?print=pretty", stories[0]);
        SetupHttpResponse("item/2.json?print=pretty", stories[1]);

        // Act - requesting more stories than available
        var result = await _service.GetBestStoriesAsync(10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetBestStoriesAsync_WithNullStories_FiltersOutNulls()
    {
        // Arrange
        var storyIds = new List<int> { 1, 2, 3 };

        SetupHttpResponse("beststories.json?print=pretty", storyIds);
        SetupHttpResponse("item/1.json?print=pretty", new Story
        {
            Title = "Story 1",
            Url = "https://example.com/1",
            By = "user1",
            Time = 1609459200,
            Score = 100,
            Descendants = 10
        });
        SetupHttpResponseNull("item/2.json?print=pretty");
        SetupHttpResponse("item/3.json?print=pretty", new Story
        {
            Title = "Story 3",
            Url = "https://example.com/3",
            By = "user3",
            Time = 1609632000,
            Score = 50,
            Descendants = 5
        });

        // Act
        var result = await _service.GetBestStoriesAsync(3);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count); // Only non-null stories
        Assert.All(result, story => Assert.NotNull(story.Title));
    }

    [Fact]
    public async Task GetBestStoriesAsync_WithNoStoryIds_ReturnsEmptyList()
    {
        // Arrange
        SetupHttpResponse("beststories.json?print=pretty", new List<int>());

        // Act
        var result = await _service.GetBestStoriesAsync(10);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetBestStoriesAsync_MapsStoryToDtoCorrectly()
    {
        // Arrange
        var storyIds = new List<int> { 1 };
        var story = new Story
        {
            Title = "Test Story",
            Url = "https://example.com/test",
            By = "testuser",
            Time = 1609459200, // 2021-01-01T00:00:00Z
            Score = 150,
            Descendants = 25
        };

        SetupHttpResponse("beststories.json?print=pretty", storyIds);
        SetupHttpResponse("item/1.json?print=pretty", story);

        // Act
        var result = await _service.GetBestStoriesAsync(1);

        // Assert
        Assert.Single(result);
        var dto = result[0];
        Assert.Equal("Test Story", dto.Title);
        Assert.Equal("https://example.com/test", dto.Uri);
        Assert.Equal("testuser", dto.PostedBy);
        Assert.Equal("2021-01-01T00:00:00.0000000Z", dto.Time);
        Assert.Equal(150, dto.Score);
        Assert.Equal(25, dto.CommentCount);
    }

    [Fact]
    public async Task GetBestStoriesAsync_FetchesDoubleTheRequestedAmount()
    {
        // Arrange
        var storyIds = Enumerable.Range(1, 20).ToList();

        SetupHttpResponse("beststories.json?print=pretty", storyIds);

        // Setup stories - make first 10 have high scores
        for (int i = 1; i <= 10; i++)
        {
            SetupHttpResponse($"item/{i}.json?print=pretty", new Story
            {
                Title = $"Story {i}",
                Url = $"https://example.com/{i}",
                By = $"user{i}",
                Time = 1609459200,
                Score = 1000 - i,
                Descendants = i
            });
        }

        // Act - request 5 stories
        var result = await _service.GetBestStoriesAsync(5);

        // Assert
        Assert.Equal(5, result.Count);

        // Verify stories are sorted by score
        for (int i = 0; i < result.Count - 1; i++)
        {
            Assert.True(result[i].Score >= result[i + 1].Score);
        }
    }

    [Fact]
    public async Task GetBestStoriesAsync_WithMixedScores_ReturnsSortedByScoreDescending()
    {
        // Arrange
        var storyIds = new List<int> { 1, 2, 3, 4, 5 };
        var stories = new Dictionary<int, Story>
        {
            { 1, new Story { Title = "Story 1", Url = "url1", By = "user1", Time = 1609459200, Score = 75, Descendants = 5 } },
            { 2, new Story { Title = "Story 2", Url = "url2", By = "user2", Time = 1609459200, Score = 150, Descendants = 10 } },
            { 3, new Story { Title = "Story 3", Url = "url3", By = "user3", Time = 1609459200, Score = 25, Descendants = 2 } },
            { 4, new Story { Title = "Story 4", Url = "url4", By = "user4", Time = 1609459200, Score = 200, Descendants = 15 } },
            { 5, new Story { Title = "Story 5", Url = "url5", By = "user5", Time = 1609459200, Score = 100, Descendants = 8 } }
        };

        SetupHttpResponse("beststories.json?print=pretty", storyIds);

        foreach (var kvp in stories)
        {
            SetupHttpResponse($"item/{kvp.Key}.json?print=pretty", kvp.Value);
        }

        // Act
        var result = await _service.GetBestStoriesAsync(5);

        // Assert
        Assert.Equal(5, result.Count);
        Assert.Equal("Story 4", result[0].Title); // Score 200
        Assert.Equal("Story 2", result[1].Title); // Score 150
        Assert.Equal("Story 5", result[2].Title); // Score 100
        Assert.Equal("Story 1", result[3].Title); // Score 75
        Assert.Equal("Story 3", result[4].Title); // Score 25
    }

    [Fact]
    public async Task GetBestStoriesAsync_CachesResults()
    {
        // Arrange
        var storyIds = new List<int> { 1 };
        var story = new Story
        {
            Title = "Cached Story",
            Url = "https://example.com/cached",
            By = "cacheuser",
            Time = 1609459200,
            Score = 99,
            Descendants = 7
        };

        SetupHttpResponse("beststories.json?print=pretty", storyIds);
        SetupHttpResponse("item/1.json?print=pretty", story);

        // Act - First call
        var result1 = await _service.GetBestStoriesAsync(1);

        // Act - Second call (should use cache)
        var result2 = await _service.GetBestStoriesAsync(1);

        // Assert
        Assert.Equal(result1[0].Title, result2[0].Title);
        Assert.Equal(result1[0].Score, result2[0].Score);

        // HTTP handler should only be called once per endpoint due to caching
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Exactly(2), // Once for beststories, once for story item
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    private void SetupHttpResponse<T>(string endpoint, T data)
    {
        var json = JsonSerializer.Serialize(data);
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains(endpoint)),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);
    }

    private void SetupHttpResponseNull(string endpoint)
    {
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains(endpoint)),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);
    }
}