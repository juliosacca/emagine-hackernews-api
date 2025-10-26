namespace EmagineHackerNewsApi.Models;

public class ResponseData
{
    public int Page { get; set; } = 1;
    public int Total { get; set; } = 0;
    public int PageSize{ get; set; } = 10;
    public IEnumerable<Story> Stories { get; set; } = Enumerable.Empty<Story>();
}