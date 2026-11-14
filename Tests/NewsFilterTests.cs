using System.Collections.Generic;
using AvaloniaAppCrypto.Models;
using AvaloniaAppCrypto.Services;
using Xunit;

namespace AvaloniaAppCrypto.Tests
{
    public class NewsFilterTests
    {
        private static NewsArticle Article(string title, string cats, string source = "Test") =>
            new() { Title = title, Categories = cats, Source = source };

        [Fact]
        public void Tumu_ReturnsEverything()
        {
            var articles = new List<NewsArticle>
            {
                Article("Bitcoin rally", "BTC"),
                Article("Ethereum merge", "ETH")
            };

            Assert.Equal(2, NewsFilter.Apply(articles, "Tümü", "").Count);
        }

        [Fact]
        public void Btc_MatchesBitcoinTitleEvenWhenCategoryMissing()
        {
            var articles = new List<NewsArticle>
            {
                Article("Bitcoin breaks resistance", ""),
                Article("Ethereum merge", "ETH")
            };

            var result = NewsFilter.Apply(articles, "BTC", "");
            Assert.Single(result);
            Assert.Contains("Bitcoin", result[0].Title);
        }

        [Fact]
        public void Eth_MatchesEthereumKeyword()
        {
            var articles = new List<NewsArticle>
            {
                Article("Ethereum staking grows", ""),
                Article("Bitcoin ETF inflows", "BTC")
            };

            var result = NewsFilter.Apply(articles, "ETH", "");
            Assert.Single(result);
        }

        [Fact]
        public void Altcoin_ExcludesBtcEthAndNonCryptoStories()
        {
            var articles = new List<NewsArticle>
            {
                Article("Bitcoin news", "BTC"),
                Article("Ethereum news", "ETH"),
                Article("Solana news", "SOL"),
                Article("Bitcoin ETF inflows grow", ""),
                Article("Microsoft confirms global outage", "")
            };

            var result = NewsFilter.Apply(articles, "Altcoin", "");
            Assert.Single(result);
            Assert.Contains("Solana", result[0].Title);
        }

        [Fact]
        public void Query_MatchesTitleSourceAndCategories_CaseInsensitive()
        {
            var articles = new List<NewsArticle>
            {
                Article("ETF inflows record", "", "CoinDesk"),
                Article("Other story", "", "Decrypt")
            };

            Assert.Single(NewsFilter.Apply(articles, "Tümü", "etf"));
            Assert.Single(NewsFilter.Apply(articles, "Tümü", "decrypt"));
        }

        [Fact]
        public void QueryAndCategory_CombineAsAnd()
        {
            var articles = new List<NewsArticle>
            {
                Article("Bitcoin ETF approval", "BTC"),
                Article("Ethereum ETF approval", "ETH")
            };

            var result = NewsFilter.Apply(articles, "BTC", "etf");
            Assert.Single(result);
            Assert.Contains("Bitcoin", result[0].Title);
        }

        [Fact]
        public void NoMatch_ReturnsEmptyList()
        {
            var articles = new List<NewsArticle> { Article("Bitcoin news", "BTC") };
            Assert.Empty(NewsFilter.Apply(articles, "DeFi", ""));
        }
    }
}
