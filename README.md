# Emagine Hacker News API

A RESTful API built with ASP.NET Core 9.0 that retrieves and ranks the best stories from Hacker News.

## Features

- Fetches top N best stories from Hacker News
- Returns stories sorted by score (highest first)
- In-memory caching for optimal performance
- Comprehensive unit tests
- Swagger/OpenAPI documentation

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Visual Studio 2022, Rider, or VS Code (optional)

## Running the Application

### Option 1: Command Line

```bash
cd EmagineHackerNewsApi
dotnet restore
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `https://localhost:5001/swagger`

### Option 2: Visual Studio / Rider

1. Open `EmagineHackerNewsApi.sln`
2. Set `EmagineHackerNewsApi` as the startup project
3. Press F5 or click Run

## API Endpoints

### Get Best Stories

```
GET /api/beststories/{n}
```

**Parameters:**
- `n` (required): Number of stories to return (1-500)

**Example:**
```bash
curl https://localhost:5001/api/beststories/10
```

**Response:**
```json
[
  {
    "title": "A uBlock Origin update was rejected from the Chrome Web Store",
    "uri": "https://github.com/uBlockOrigin/uBlock-issues/issues/745",
    "postedBy": "ismaildonmez",
    "time": "2019-10-12T13:43:01.0000000Z",
    "score": 1716,
    "commentCount": 572
  }
]
```

## Running Tests

```bash
cd EmagineHackerNewsTests
dotnet test
```

Or run all tests from the solution root:
```bash
dotnet test
```

## Project Structure

```
EmagineHackerNewsApi/
├── Controllers/          # API endpoints
├── Services/            # Business logic
├── Models/              # Data models
├── DTOs/                # Data transfer objects
├── appsettings.json     # Configuration

EmagineHackerNewsTests/
├── Controllers/         # Controller tests
├── Services/            # Service tests
```

## Caching Strategy

- **Story IDs cache**: 30 seconds (frequently changing list)
- **Individual stories cache**: 30 minutes (rarely change once published)

This approach balances freshness with performance, reducing load on the Hacker News API.

## Assumptions

1. **Valid story data**: Assumes Hacker News API returns well-formed data
2. **Network reliability**: Basic error handling for HTTP failures
3. **Cache invalidation**: Stories are considered stable after 30 minutes
4. **Concurrent requests**: Uses `Task.WhenAll` for parallel story fetching
5. **Over-fetching**: Fetches 2x requested stories to handle potential nulls/invalid entries

## Enhancements (Given More Time)

### Performance
- Implement distributed caching (Redis) for multi-instance deployments
- Add response compression
- Implement request throttling/rate limiting

### Reliability
- Add circuit breaker pattern for external API calls (Polly)
- Implement retry logic with exponential backoff
- Add comprehensive logging (Serilog)
- Implement health checks

### Features
- Pagination support for large result sets
- Filter by date range, author, or score threshold
- Search functionality
- Background job to pre-populate cache
- Support for other Hacker News endpoints (new, top, ask, show, job)

### Testing
- Integration tests with real Hacker News API
- Load testing to verify performance under stress
- Add test coverage reporting

### Security
- Add API key authentication
- Implement CORS policies properly
- Add request validation middleware

### Monitoring
- Application Insights or similar APM
- Metrics for cache hit rates
- Response time tracking
- Error rate monitoring

## Configuration

Edit `appsettings.json` to change the Hacker News API base URL:

```json
{
  "HackerNewsApi": {
    "BaseUrl": "https://hacker-news.firebaseio.com/v0/"
  }
}
```

## Contact

For questions or issues, please open a GitHub issue.