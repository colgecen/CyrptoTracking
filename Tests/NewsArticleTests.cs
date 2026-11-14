using System;
using AvaloniaAppCrypto.Models;
using Xunit;

namespace AvaloniaAppCrypto.Tests
{
    public class NewsArticleTests
    {
        [Fact]
        public void PlainBody_StripsHtmlAndDecodesEntities()
        {
            var article = new NewsArticle
            {
                Body = "<p>Bitcoin &amp; Ether <b>yükseldi</b>.</p>"
            };

            Assert.Equal("Bitcoin & Ether yükseldi.", article.PlainBody);        }

        [Fact]
        public void PlainBody_FallsBackWhenBodyIsEmpty()
        {
            var article = new NewsArticle { Body = "" };

            Assert.False(string.IsNullOrWhiteSpace(article.PlainBody));
        }

        [Fact]
        public void PublishedOnText_UsesReadableFormat()
        {
            var article = new NewsArticle
            {
                PublishedOn = new DateTime(2026, 11, 14, 14, 20, 0, DateTimeKind.Utc)
            };

            Assert.Contains("2026", article.PublishedOnText);
            Assert.Contains(":", article.PublishedOnText);
        }
    }
}
