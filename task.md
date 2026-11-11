# CryptoTrack — Kusursuz İyileştirme Planı

> Bu plan, uygulamanın "düzen, çalışma sistemi, grafikler, canlı veri, API anahtarı yönetimi ve yenileme" konularındaki tüm sorunlarını çözmek için hazırlanmıştır.  
> Her görev tamamlandığında `agents.md` kurallarına göre **geleneksel commit + push** yapılacaktır.

## Durum Göstergeleri
- `[ ]` Bekliyor
- `[~]` Devam ediyor
- `[x]` Tamamlandı
- `[!]` Bloke / karar bekliyor

---

## Faz 0 — Temel ve Repo Hijyeni

- [ ] **T0.1** Proje build/test temelini doğrula (`dotnet build`, `dotnet run`).
- [ ] **T0.2** `.gitignore` ve build çıktılarını temizle; repo'ya `bin/`, `obj/` girmesini engelle.
- [ ] **T0.3** `README.md` kurulum adımlarını Linux/macOS/Windows için netleştir.
- [ ] **T0.4** `task.md` ve `agents.md` dosyalarını repo kökünde tut ve güncel tut.
- [ ] **T0.5** Git branch stratejisi belirle (`main`, `develop`, `feature/*`, `fix/*`).

---

## Faz 1 — Mimari, Ayarlar ve API Anahtarı Yönetimi

- [x] **T1.1** Hardcoded API anahtarlarını servislerden kaldır; ayarlardan oku (CoinGecko, CryptoCompare).
- [x] **T1.2** `AppSettings` modeline API anahtarı alanları ekle (CoinGecko, CryptoCompare, isteğe bağlı Binance/CoinCap).
- [x] **T1.3** Ayarlar ekranına "API Anahtarları" bölümü ekle; alanlar maskelendi.
- [ ] **T1.4** API anahtarlarını güvenli şekilde sakla (mümkünse OS credential store; yoksa yerel şifreli JSON).
- [ ] **T1.5** API anahtarı eksikse uygulama çökmesin; uyarı + demo/mod degrade modu.
- [ ] **T1.6** Servislerin DI üzerinden ayarları okumasını sağla (her istekte güncel anahtar).
- [ ] **T1.7** "Ayarları Test Et" butonu ekle (bağlantı/ping + anahtar doğrulama).
- [ ] **T1.8** Varsayılan endpoint ve timeout ayarlarını yapılandırılabilir yap.

---

## Faz 2 — UI/UX ve Düzen (En Büyük Sorun Alanı)

- [ ] **T2.1** Tüm ekranlarda hardcoded renkleri tema kaynaklarına (`DynamicResource`) geçir.
- [ ] **T2.2** Ana pencere düzenini yeniden tasarla: header, sidebar, content, status bar.
- [ ] **T2.3** Sidebar daraltma/genişletme animasyonu ve ikonlu menü.
- [ ] **T2.4** Responsive davranış: küçük ekranlarda sidebar overlay, kartlar wrap.
- [ ] **T2.5** Dashboard kartlarını grid ile hizala; boşluk/typography tutarlılığı.
- [ ] **T2.6** Coin tablosunu gerçek tablo gibi yap (sıralama, hizalama, zebra, hover).
- [ ] **T2.7** Formlar (portföy, alarm) için modal/dialog ve validasyon.
- [ ] **T2.8** Toast/notification sistemi tutarlı ve erişilebilir yap.
- [ ] **T2.9** Dark/Light tema geçişini anında ve kusursuz yap.
- [ ] **T2.10** Yükleme durumları (skeleton/spinner) ve boş durum (empty state) tasarımları.
- [ ] **T2.11** Hata durumları için kullanıcı dostu mesajlar ve yeniden deneme butonu.
- [ ] **T2.12** Erişilebilirlik: klavye navigasyonu, focus göstergeleri, kontrast.

---

## Faz 3 — Veri Katmanı ve Servis Mimarisi

- [ ] **T3.1** Servis arayüzlerini genişlet (refresh, cancel, retry, health).
- [ ] **T3.2** HTTP client factory/pool ve timeout/retry politikası (Polly benzeri).
- [ ] **T3.3** Rate limit ve quota yönetimi (CoinGecko/CryptoCompare).
- [ ] **T3.4** Önbellek katmanı (in-memory + disk cache) ve TTL.
- [ ] **T3.5** Veri modellerini normalize et (Coin, Market, Portfolio, Alert, News).
- [ ] **T3.6** WebSocket bağlantı yönetimi: reconnect, backoff, durum bildirimi.
- [ ] **T3.7** Binance/CoinCap fallback mantığını güvenilir hale getir.
- [ ] **T3.8** Global market verisi (market cap, volume, BTC dominance) gerçek API'ye bağla.
- [ ] **T3.9** Fear & Greed index gerçek veri kaynağına bağla.
- [ ] **T3.10** Hata loglama (structured logging) ve telemetri (opsiyonel).

---

## Faz 4 — Yenileme (Refresh) Sistemi

- [ ] **T4.1** Tüm veri yükleyen ekranlara manuel "Yenile" butonu ekle.
- [ ] **T4.2** Pull-to-refresh / F5 kısayolu desteği.
- [ ] **T4.3** Otomatik yenileme (ayarlanabilir interval: 10s/30s/60s/kapalı).
- [ ] **T4.4** Yenileme sırasında UI kilitlenmesin; async + progress göstergesi.
- [ ] **T4.5** Kısmi yenileme (sadece görünür sayfa) ve tam yenileme.
- [ ] **T4.6** WebSocket canlı güncellemeler ile tablo/kartların anlık güncellenmesi.
- [ ] **T4.7** Yenileme sonrası scroll pozisyonu ve seçim korunsun.
- [ ] **T4.8** Son güncelleme zamanı ("Son güncelleme: ...") göster.

---

## Faz 5 — Grafikler ve Görselleştirme (Çok Çeşitli, Gerçek/Canlı)

- [ ] **T5.1** Sparkline bileşenini gelişmiş yap (gradient, tooltip, min/max işaretleri).
- [ ] **T5.2** BTC için alan grafiği (area chart) + zaman aralığı seçimi (1g/7g/30g/90g/1y).
- [ ] **T5.3** Mum grafiği (candlestick) ekle (OHLC).
- [ ] **T5.4** Çizgi grafiği (line) + çoklu seri (BTC, ETH karşılaştırma).
- [ ] **T5.5** Bar/sütun grafikleri (hacim, top gainers/losers).
- [ ] **T5.6** Portföy dağılımı için pie/donut chart.
- [ ] **T5.7** Piyasa dominansı için donut chart.
- [ ] **T5.8** Fear & Greed için gauge chart.
- [ ] **T5.9** Grafiklerde zoom/pan, tooltip, legend, export (PNG/SVG).
- [ ] **T5.10** Grafikler canlı veri ile animasyonlu güncellensin.
- [ ] **T5.11** Grafik performansı: büyük veri setlerinde down-sampling.
- [ ] **T5.12** Grafik teması dark/light ile uyumlu.

---

## Faz 6 — Özellik Tamamlama (Portföy, Alarmlar, Favoriler, Haberler)

- [ ] **T6.1** Portföy ekleme/düzenleme/silme formları tamamla.
- [ ] **T6.2** Portföy P/L ve yüzde hesaplamalarını doğrula.
- [ ] **T6.3** Portföy için maliyet temeli ve realized/unrealized P/L.
- [ ] **T6.4** Alarm oluşturma (üst/alt eşik, yüzde değişim) tamamla.
- [ ] **T6.5** Alarm tetikleme ve bildirim (toast + ses/OS notification).
- [ ] **T6.6** Tetiklenen alarmların arşivlenmesi.
- [ ] **T6.7** Favoriler görünümü ve hızlı ekleme/çıkarma.
- [ ] **T6.8** Haberler görünümü: filtre, arama, kategori, aç.
- [ ] **T6.9** Haber görselleri ve okuma süresi.
- [ ] **T6.10** Arama (coin) gelişmiş: debounce, sonuç vurgulama.

---

## Faz 7 — Canlılık, Güvenilirlik ve Performans

- [ ] **T7.1** Uygulama başlangıç süresini optimize et (lazy init).
- [ ] **T7.2** UI thread bloklamasını engelle (async/await, dispatcher).
- [ ] **T7.3** Bellek sızıntılarını kontrol et (event unsubscribe).
- [ ] **T7.4** Offline mod: önbellekten veri göster.
- [ ] **T7.5** Ağ hatası retry/backoff ve kullanıcı bilgilendirmesi.
- [ ] **T7.6** Çökme raporlama (unhandled exception handler).
- [ ] **T7.7** Performans profilleme ve optimizasyon.

---

## Faz 8 — Test, Kalite ve CI/CD

- [ ] **T8.1** Unit test projesi oluştur.
- [ ] **T8.2** Servisler için mock testler.
- [ ] **T8.3** ViewModel testleri.
- [ ] **T8.4** UI smoke testleri (mümkünse).
- [ ] **T8.5** GitHub Actions: build + test (Windows/Linux/macOS).
- [ ] **T8.6** Kod analizi/lint (Roslyn analyzers).
- [ ] **T8.7** Release pipeline (publish artifacts).

---

## Faz 9 — Dokümantasyon ve Yayın

- [ ] **T9.1** Kullanıcı rehberi (TR/EN).
- [ ] **T9.2** Geliştirici rehberi ve mimari doküman.
- [ ] **T9.3** Ekran görüntüleri/GIF'ler.
- [ ] **T9.4** Sürüm notları (CHANGELOG).
- [ ] **T9.5** Lisans ve katkı rehberi güncellemesi.

---

## Öncelik Sırası (İlk 10 Görev)
1. T1.1 API anahtarlarını hardcoded olmaktan çıkar.
2. T1.3 API anahtarı giriş UI.
3. T4.1 Yenile butonu.
4. T2.1 Tema kaynaklarına geçiş.
5. T3.8 Global market verisi gerçek API.
6. T5.2 BTC alan grafiği + zaman aralığı.
7. T6.1 Portföy formu.
8. T6.4 Alarm formu.
9. T2.6 Coin tablosu.
10. T8.1 Test projesi.

---

## Kabul Kriterleri (Genel)
- Uygulama Linux/macOS/Windows'ta hatasız açılır.
- API anahtarları kaynak koda gömülü değildir.
- Tüm ana ekranlarda manuel yenileme vardır.
- En az 3 farklı grafik türü (sparkline, area, donut/pie) çalışır ve canlı güncellenir.
- Dark/Light tema anında ve tutarlıdır.
- Build/test CI'da yeşil geçer.
