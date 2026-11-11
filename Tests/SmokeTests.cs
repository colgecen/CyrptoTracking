using Xunit;
using AvaloniaAppCrypto.Models;

namespace AvaloniaAppCrypto.Tests
{
    public class SmokeTests
    {
        [Fact]
        public void AppSettings_Defaults_AreValid()
        {
            var settings = new AppSettings();
            Assert.True(settings.RefreshIntervalSeconds > 0);
            Assert.False(string.IsNullOrWhiteSpace(settings.Theme));
        }

        [Fact]
        public void ChartSegment_StoresValues()
        {
            var seg = new ChartSegment { Label = "BTC", Value = 42, Color = "#3B82F6" };
            Assert.Equal("BTC", seg.Label);
            Assert.Equal(42, seg.Value);
        }

        [Fact]
        public void PriceAlert_Defaults_AreValid()
        {
            var alert = new PriceAlert { CoinId = "bitcoin", TargetPrice = 100, Condition = "Above" };
            Assert.True(alert.IsActive);
            Assert.Equal("Above", alert.Condition);
        }

        [Fact]
        public void PortfolioEntry_StoresValues()
        {
            var entry = new PortfolioEntry { CoinId = "bitcoin", Amount = 2, AverageBuyPrice = 50000 };
            Assert.Equal(2, entry.Amount);
            Assert.Equal(50000, entry.AverageBuyPrice);
        }
    }
}