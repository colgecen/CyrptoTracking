using System;
using System.Net;
using System.Text.RegularExpressions;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaAppCrypto.Models
{
    public partial class NewsArticle : ObservableObject
    {
        public string Title { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime PublishedOn { get; set; } = DateTime.Now;
        public string Categories { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;

        /// <summary>Kapak fotoğrafı; API'den indirilip buraya atanır.</summary>
        [ObservableProperty] private IImage? _coverImage;

        public string PublishedOnText =>
            (PublishedOn.Kind == DateTimeKind.Utc ? PublishedOn.ToLocalTime() : PublishedOn)
                .ToString("dd MMM yyyy · HH:mm");

        private string? _plainBody;
        public string PlainBody => _plainBody ??= BuildPlainBody();

        private string BuildPlainBody()
        {
            if (string.IsNullOrWhiteSpace(Body))
                return "Bu haber için özet metni bulunmuyor. Kaynağa giderek haberin tamamını okuyabilirsiniz.";

            // Gövde HTML olarak geliyor: etiketleri temizle, kaçış karakterlerini çöz.
            var text = Regex.Replace(Body, "<[^>]+>", " ");
            text = WebUtility.HtmlDecode(text);
            text = Regex.Replace(text, @"\s+", " ").Trim();
            // Etiket kaldırmadan kalan boşlukları noktalama öncesinde temizle.
            text = Regex.Replace(text, @"\s+([.,!?;:])", "$1");
            return text;
        }
    }
}
