/**
 * =============================================================================
 * DOSYA: app.config.ts
 * AÇIKLAMA:
 * Bu dosya, modern Angular uygulamalarında "merkezi ayarlar paneli" veya "kurulum kutusu"
 * görevi görür.
 * 
 * Uygulamanın çalışırken ihtiyaç duyacağı evrensel yetenekler (örneğin internetten veri çekme,
 * genel hata yakalama, yönlendirme ayarları vb.) burada "Provider" (Sağlayıcı) olarak tanımlanır.
 * 
 * Bu yapıya yazılım dünyasında "Dependency Injection" (Bağımlılık Enjeksiyonu) denir:
 * İhtiyacımız olan araçları bir havuza koyarız, Angular bu araçları isteyen her yere
 * otomatik olarak dağıtır.
 * =============================================================================
 */

// 1. İÇE AKTARMALAR (IMPORTS):
// Angular'ın çekirdek kütüphanesinden tip tanımı ve hata dinleyicisi fonksiyonunu alıyoruz.
import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';

// Angular'ın HTTP kütüphanesinden, internet üzerinden (arka uç sunucularından / API'lerden)
// veri alıp gönderebilmemizi sağlayan hazır aracı içeri aktarıyoruz.
import { provideHttpClient } from '@angular/common/http';

/**
 * 2. UYGULAMA YAPILANDIRMA NESNESİ (appConfig):
 * 
 * 'export': Bu ayar nesnesini diğer dosyaların da (özellikle main.ts'in) görebilmesini sağlar.
 * 'const': Değişmez (constant) bir değişken tanımlar; uygulama çalıştığı sürece bu ayar nesnesi başka bir şeyle değiştirilemez.
 * ': ApplicationConfig': TypeScript tip tanımıdır. Bu değişkenin Angular'ın istediği konfigürasyon kurallarına uyduğunu garanti eder.
 */
export const appConfig: ApplicationConfig = {
  // 'providers' (Sağlayıcılar): Uygulamaya hangi özellikleri ve yetenekleri kazandırmak istediğimizi belirttiğimiz listedir (dizidir).
  providers: [
    // Tarayıcıdaki global JavaScript hatalarını dinleyip yakalamayı sağlayan Angular mekanizması:
    provideBrowserGlobalErrorListeners(),

    // Uygulamanın backend (C# Web API vb.) sunucularıyla HTTP (GET, POST gibi) protokolü üzerinden
    // haberleşebilmesi için gereken HttpClient servisini aktif hale getirir:
    provideHttpClient(),
  ]
};
