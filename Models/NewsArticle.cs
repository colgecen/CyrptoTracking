using System;

namespace AvaloniaAppCrypto.Models
{
    public class NewsArticle
    {
        public string Title { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime PublishedOn { get; set; } = DateTime.Now;
        public string Categories { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }
}
