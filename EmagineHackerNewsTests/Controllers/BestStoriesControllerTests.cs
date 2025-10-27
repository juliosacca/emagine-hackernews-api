using EmagineHackerNewsApi.Controllers;
using EmagineHackerNewsApi.DTOs;
using EmagineHackerNewsApi.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EmagineHackerNewsApi.Tests.Controllers;

public class BestStoriesControllerTests
{
    private readonly Mock<IStoriesService> _mockStoriesService;
    private readonly BestStoriesController _controller;

    public BestStoriesControllerTests()
    {
        _mockStoriesService = new Mock<IStoriesService>();
        _controller = new BestStoriesController(_mockStoriesService.Object);
    }

    [Fact]
    public async Task GetBestStoriesAsync_WithValidN_ReturnsOkWithStories()
    {
        // Arrange
        int n = 5;
        var expectedStories = new List<StoryDto>
        {
            new StoryDto
            {
                Title = "Story 1",
                Uri = "https://example.com/1",
                PostedBy = "user1",
                Time = "2024-01-01T00:00:00Z",
                Score = 100,
                CommentCount = 10
            },
            new StoryDto
            {
                Title = "Story 2",
                Uri = "https://example.com/2",
                PostedBy = "user2",
                Time = "2024-01-02T00:00:00Z",
                Score = 90,
                CommentCount = 5
            }
        };

        _mockStoriesService
            .Setup(s => s.GetBestStoriesAsync(n, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStories);

        // Act
        var result = await _controller.GetBestStoriesAsync(n);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var stories = Assert.IsType<List<StoryDto>>(okResult.Value);
        Assert.Equal(2, stories.Count);
        Assert.Equal("Story 1", stories[0].Title);

        _mockStoriesService.Verify(s => s.GetBestStoriesAsync(n, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task GetBestStoriesAsync_WithZeroOrNegativeN_ReturnsBadRequest(int n)
    {
        // Act
        var result = await _controller.GetBestStoriesAsync(n);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("number of the top n stories must be greater than zero.", badRequestResult.Value);

        _mockStoriesService.Verify(s => s.GetBestStoriesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetBestStoriesAsync_WithNGreaterThan500_ReturnsBadRequest()
    {
        // Arrange
        int n = 501;

        // Act
        var result = await _controller.GetBestStoriesAsync(n);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("number of the top n stories cannot be greater than 500.", badRequestResult.Value);

        _mockStoriesService.Verify(s => s.GetBestStoriesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetBestStoriesAsync_WhenNoStoriesFound_ReturnsNotFound()
    {
        // Arrange
        int n = 10;
        _mockStoriesService
            .Setup(s => s.GetBestStoriesAsync(n, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StoryDto>());

        // Act
        var result = await _controller.GetBestStoriesAsync(n);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("No stories found.", notFoundResult.Value);
    }

    [Fact]
    public async Task GetBestStoriesAsync_WhenServiceReturnsNull_ReturnsNotFound()
    {
        // Arrange
        int n = 10;
        _mockStoriesService
            .Setup(s => s.GetBestStoriesAsync(n, It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<StoryDto>?)null);

        // Act
        var result = await _controller.GetBestStoriesAsync(n);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("No stories found.", notFoundResult.Value);
    }

    [Fact]
    public async Task GetBestStoriesAsync_WithMixedNullStories_FiltersOutNulls()
    {
        // Arrange
        int n = 5;
        var storiesWithNulls = new List<StoryDto?>
        {
            new StoryDto { Title = "Story 1", Score = 100 },
            null,
            new StoryDto { Title = "Story 2", Score = 90 }
        };

        _mockStoriesService
            .Setup(s => s.GetBestStoriesAsync(n, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storiesWithNulls.Where(s => s != null).Select(s => s!).ToList());

        // Act
        var result = await _controller.GetBestStoriesAsync(n);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var stories = Assert.IsType<List<StoryDto>>(okResult.Value);
        Assert.Equal(2, stories.Count);
        Assert.All(stories, s => Assert.NotNull(s));
    }
    
}