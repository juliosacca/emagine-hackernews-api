namespace EmagineHackerNewsApi.DTOs
{
    /// <summary>
    /// Represents a Hacker News story with relevant metadata.
    /// </summary>
    public class StoryDto
    {
        /// <summary>
        /// The title of the story.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The URL of the story.
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// The username of the author who posted the story.
        /// </summary>
        public string PostedBy { get; set; }

        /// <summary>
        /// The time the story was posted, formatted as an ISO 8601 string.
        /// </summary>
        public string Time { get; set; }

        /// <summary>
        /// The score of the story, based on user upvotes.
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// The number of comments on the story.
        /// </summary>
        public int CommentCount { get; set; }
    }
}