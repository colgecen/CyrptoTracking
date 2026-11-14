using System;
using System.Collections.Generic;
using System.Linq;
using AvaloniaAppCrypto.Models;

namespace AvaloniaAppCrypto.Services
{
    /// <summary>
    /// Haber listesi filtreleme kuralları. ViewModel'den ayrı tutulur ki
    /// test edilebilsin.
    /// </summary>
    public static class NewsFilter
    {
        public static bool MatchesCategory(NewsArticle article, string? category)
        {
            if (article is null) return false;
            if (string.IsNullOrWhiteSpace(category) || category == "Tümü")
                return true;

            var haystack = $"{article.Categories} {article.Title}";

            bool Has(params string[] keys) =>
                keys.Any(k => haystack.Contains(k, StringComparison.OrdinalIgnoreCase));

            if (string.Equals(category, "Altcoin", StringComparison.OrdinalIgnoreCase))
            {
                var cats = article.Categories ?? string.Empty;
                return !cats.Contains("BTC", StringComparison.OrdinalIgnoreCase) &&
                       !cats.Contains("Bitcoin", StringComparison.OrdinalIgnoreCase) &&
                       !cats.Contains("ETH", StringComparison.OrdinalIgnoreCase) &&
                       !cats.Contains("Ethereum", StringComparison.OrdinalIgnoreCase);
            }

            return category switch
            {
                "BTC" => Has("BTC", "Bitcoin"),
                "ETH" => Has("ETH", "Ethereum", "Ether"),
                "DeFi" => Has("DeFi", "Decentralized Finance", "Uniswap", "Aave", "Maker"),
                _ => Has(category)
            };
        }

        public static bool MatchesQuery(NewsArticle article, string? query)
        {
            if (article is null) return false;
            var q = query?.Trim();
            if (string.IsNullOrEmpty(q)) return true;

            return (article.Title?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                   (article.Source?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                   (article.Categories?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false);
        }

        public static List<NewsArticle> Apply(
            IEnumerable<NewsArticle> articles, string? category, string? query)
        {
            return articles
                .Where(a => MatchesCategory(a, category) && MatchesQuery(a, query))
                .ToList();
        }
    }
}
