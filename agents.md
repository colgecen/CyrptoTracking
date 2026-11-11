# CryptoTrack — Agent Çalışma Kuralları (agents.md)

Bu dosya, plan (`task.md`) üzerinde çalışan agent'ların **rol, süreç, kalite ve commit/push** kurallarını tanımlar.

## 1) Genel İlkeler

- Her değişiklik `task.md` içindeki bir görev (Task ID) ile eşleşmelidir.
- Bir görev tamamlanmadan yeni göreve geçilmez.
- Her görev tamamlandığında **geleneksel commit + push** yapılır.
- Kod, mevcut mimari ve stil ile uyumlu olmalıdır (MVVM, DI, CommunityToolkit.Mvvm).
- API anahtarları **asla** kaynak koda gömülmez.
- Kullanıcı verisi ve ayarlar platform bağımsız yolda saklanır.

---

## 2) Görev Yaşam Döngüsü

1. **Seç**: `task.md`'den en yüksek öncelikli `[ ]` görevi seç.
2. **İşaretle**: Görevi `[~]` (devam ediyor) yap.
3. **Planla**: Gerekli dosyaları ve riskleri not et.
4. **Uygula**: Küçük, odaklı değişiklikler yap.
5. **Doğrula**:
   - `dotnet build` başarılı olmalı.
   - Mümkünse `dotnet run` ile manuel kontrol.
   - İlgili testler çalıştırılmalı.
6. **Commit**: Aşağıdaki formatla commit at.
7. **Push**: Uzak branch'e push et.
8. **Güncelle**: Görevi `[x]` yap ve kısa not ekle.
9. **Raporla**: Kullanıcıya tamamlanan görev ve commit hash'ini bildir.

---

## 3) Commit ve Push Kuralları (Zorunlu)

Her görev tamamlandığında:

```bash
git add -A
git commit -m "feat(scope): kısa açıklama (Task ID)"
git push origin <branch>
```

### Commit Mesaj Formatı
- `feat:` yeni özellik
- `fix:` hata düzeltme
- `refactor:` yeniden düzenleme
- `style:` UI/stil
- `docs:` dokümantasyon
- `test:` test
- `chore:` altyapı/yapılandırma

Örnekler:
- `feat(settings): add API key management UI (T1.3)`
- `fix(ui): replace invalid LinearGradient brush string (T2.1)`
- `feat(charts): add BTC area chart with time range (T5.2)`

### Kurallar
- Bir commit tek bir mantıksal görevi içermelidir.
- Build kırıkken commit atılmaz.
- Gizli bilgi (API key, token) commit edilmez.
- Her görev sonrası push zorunludur.

---

## 4) Agent Roller

### A. Architect Agent
- Sorumluluk: Mimari, DI, servis sınırları, veri modelleri.
- Çıktı: Arayüzler, servisler, model güncellemeleri.
- Kontrol: Build + temel akış.

### B. UI/UX Agent
- Sorumluluk: XAML düzen, tema, responsive, erişilebilirlik.
- Çıktı: View'lar, styles, converters.
- Kontrol: Görsel tutarlılık, dark/light.

### C. Data & Services Agent
- Sorumluluk: API entegrasyonları, WebSocket, cache, retry.
- Çıktı: Service sınıfları, modeller.
- Kontrol: Ağ hataları ve fallback.

### D. Charting Agent
- Sorumluluk: Grafik bileşenleri, canlı veri bağlama, performans.
- Çıktı: Controls/, chart view'lar.
- Kontrol: Render performansı, tooltip/legend.

### E. QA Agent
- Sorumluluk: Testler, build, smoke test, regresyon.
- Çıktı: Test projeleri, CI.
- Kontrol: Yeşil build/test.

### F. DevOps Agent
- Sorumluluk: CI/CD, publish, platform bağımlılıkları.
- Çıktı: `.github/workflows`, publish scripts.
- Kontrol: Çoklu platform build.

### G. Docs Agent
- Sorumluluk: README, kullanıcı/geliştirici rehberi.
- Çıktı: Dokümanlar.
- Kontrol: Güncellik.

---

## 5) Güvenlik Kuralları

- API anahtarları kullanıcı ayarlarından alınır.
- Anahtarlar maskelenir; "göster/gizle" seçeneği olur.
- Mümkünse OS credential store kullanılır; değilse yerel şifreli dosya.
- Loglarda API anahtarı yazdırılmaz.
- `.gitignore` ile yerel ayar dosyaları korunur.

---

## 6) Kalite Kapısı (Her Görev İçin)

Bir görev "tamamlandı" sayılmadan önce:

- [ ] `dotnet build` başarılı.
- [ ] İlgili ekran/özellik manuel olarak kontrol edildi.
- [ ] Regresyon yok (mevcut özellikler çalışıyor).
- [ ] Commit mesajı Task ID içeriyor.
- [ ] Push yapıldı.
- [ ] `task.md` güncellendi.

---

## 7) Branch Stratejisi

- `main`: kararlı yayın
- `develop`: entegrasyon
- `feature/<task-id>-short-name`: yeni özellik
- `fix/<task-id>-short-name`: hata düzeltme

Örnek: `feature/T5.2-btc-area-chart`

---

## 8) İletişim ve Raporlama

Her görev tamamlandığında kullanıcıya şu bilgi verilir:
- Task ID ve başlık
- Değişen dosyalar
- Commit hash
- Push edilen branch
- Sonraki önerilen görev
