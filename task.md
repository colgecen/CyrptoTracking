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

- [x] **T0.1** Proje build/run doğrulandı.
- [x] **T0.2** `.gitignore` build çıktılarını engelliyor.
- [x] **T0.3** README Linux kurulum adımları eklendi.
- [ ] **T0.4** `task.md` ve `agents.md` dosyalarını repo kökünde tut ve güncel tut.
- [x] **T0.5** Branch stratejisi agents.md'de tanımlı.

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

- [x] **T2.1** Hardcoded renkler tema kaynaklarına geçirildi.
- [ ] **T2.2** Ana pencere düzenini yeniden tasarla: header, sidebar, content, status bar.
- [ ] **T2.3** Sidebar daraltma/genişletme animasyonu ve ikonlu menü.
- [ ] **T2.4** Responsive davranış: küçük ekranlarda sidebar overlay, kartlar wrap.
- [x] **T2.5** Dashboard kartları grid ile hizalı.
- [x] **T2.6** Coin tablosuna sıralama başlıkları eklendi.
- [x] **T2.7** Formlar inline ve validasyonlu.
- [x] **T2.8** Toast/notification sistemi mevcut.
- [x] **T2.9** Tema geçişi servis ile çalışıyor.
- [x] **T2.10** Yükleme/boş durum tasarımları mevcut.
- [x] **T2.11** Hata durumlarında kullanıcı mesajları mevcut.
- [ ] **T2.12** Erişilebilirlik: klavye navigasyonu, focus göstergeleri, kontrast.

---

## Faz 3 — Veri Katmanı ve Servis Mimarisi

- [ ] **T3.1** Servis arayüzlerini genişlet (refresh, cancel, retry, health).
- [ ] **T3.2** HTTP client factory/pool ve timeout/retry politikası (Polly benzeri).
- [ ] **T3.3** Rate limit ve quota yönetimi (CoinGecko/CryptoCompare).
- [ ] **T3.4** Önbellek katmanı (in-memory + disk cache) ve TTL.
- [ ] **T3.5** Veri modellerini normalize et (Coin, Market, Portfolio, Alert, News).
- [x] **T3.6** WebSocket reconnect ayarları mevcut.
- [x] **T3.7** Binance/CoinCap fallback mevcut.
- [x] **T3.8** Global market verisi gerçek API'ye bağlı.
- [x] **T3.9** Fear & Greed index gerçek veri kaynağına bağlı.
- [x] **T3.10** Çökme loglama mevcut.

---

## Faz 4 — Yenileme (Refresh) Sistemi

- [x] **T4.1** Global manuel "Yenile" butonu eklendi.
- [ ] **T4.2** Pull-to-refresh / F5 kısayolu desteği.
- [x] **T4.3** Otomatik yenileme (ayarlanabilir interval) eklendi.
- [x] **T4.4** Yenileme async; UI kilitlenmez.
- [x] **T4.5** Tam yenileme (RefreshAll) mevcut.
- [x] **T4.6** WebSocket canlı güncellemeler ile fiyatlar anlık güncelleniyor.
- [ ] **T4.7** Yenileme sonrası scroll pozisyonu ve seçim korunsun.
- [x] **T4.8** Son güncelleme zamanı gösteriliyor.

---

## Faz 5 — Grafikler ve Görselleştirme (Çok Çeşitli, Gerçek/Canlı)

- [x] **T5.1** Sparkline gradient fill ile geliştirildi.
- [x] **T5.2** BTC grafik zaman aralığı seçimi eklendi (1G/1H/1Ay/3Ay/1Y).
- [x] **T5.3** BTC mum grafiği (candlestick) eklendi.
- [x] **T5.4** Çoklu serili çizgi grafiği (BTC vs ETH) eklendi.
- [x] **T5.5** Top gainers bar grafiği eklendi.
- [x] **T5.6** Portföy dağılımı için donut chart eklendi.
- [x] **T5.7** Piyasa dominansı için donut chart eklendi.
- [x] **T5.8** Fear & Greed için gauge chart eklendi.
- [ ] **T5.9** Grafiklerde zoom/pan, tooltip, legend, export (PNG/SVG).
- [x] **T5.10** Grafikler yenileme ile güncelleniyor.
- [ ] **T5.11** Grafik performansı: büyük veri setlerinde down-sampling.
- [ ] **T5.12** Grafik teması dark/light ile uyumlu.

---

## Faz 6 — Özellik Tamamlama (Portföy, Alarmlar, Favoriler, Haberler)

- [x] **T6.1** Portföy ekleme formu tamamlandı (düzenleme devam).
- [x] **T6.2** Portföy P/L ve yüzde hesaplamaları mevcut.
- [ ] **T6.3** Portföy için maliyet temeli ve realized/unrealized P/L.
- [x] **T6.4** Alarm oluşturma (üst/alt eşik) formu eklendi.
- [x] **T6.5** Alarm tetikleme toast bildirimi eklendi.
- [x] **T6.6** Tetiklenen alarmlar ayrı listede gösteriliyor.
- [x] **T6.7** Favoriler görünümü ve hızlı ekleme/çıkarma mevcut.
- [x] **T6.8** Haberler veri yükleme eklendi (filtre/arama devam).
- [x] **T6.9** Haber görselleri modelde mevcut.
- [x] **T6.10** Arama debounce eklendi.

---

## Faz 7 — Canlılık, Güvenilirlik ve Performans

- [ ] **T7.1** Uygulama başlangıç süresini optimize et (lazy init).
- [x] **T7.2** Async/await kullanılıyor.
- [ ] **T7.3** Bellek sızıntılarını kontrol et (event unsubscribe).
- [ ] **T7.4** Offline mod: önbellekten veri göster.
- [ ] **T7.5** Ağ hatası retry/backoff ve kullanıcı bilgilendirmesi.
- [x] **T7.6** Çökme raporlama (unhandled exception handler) eklendi.
- [ ] **T7.7** Performans profilleme ve optimizasyon.

---

## Faz 8 — Test, Kalite ve CI/CD

- [x] **T8.1** Unit test projesi oluşturuldu ve çalışıyor.
- [ ] **T8.2** Servisler için mock testler.
- [ ] **T8.3** ViewModel testleri.
- [ ] **T8.4** UI smoke testleri (mümkünse).
- [x] **T8.5** GitHub Actions build workflow eklendi.
- [ ] **T8.6** Kod analizi/lint (Roslyn analyzers).
- [x] **T8.7** Release pipeline eklendi.

---

## Faz 9 — Dokümantasyon ve Yayın

- [x] **T9.1** Kullanıcı rehberi eklendi.
- [x] **T9.2** Geliştirici rehberi eklendi.
- [ ] **T9.3** Ekran görüntüleri/GIF'ler.
- [x] **T9.4** CHANGELOG eklendi.
- [x] **T9.5** Katkı rehberi branch stratejisi ile güncellendi.

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
