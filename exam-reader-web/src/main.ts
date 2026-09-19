/**
 * =============================================================================
 * DOSYA: main.ts
 * AÇIKLAMA:
 * Bu dosya, tüm Angular uygulamasının çalışmaya başladığı İLK TypeScript dosyasıdır.
 * Tıpkı bir arabanın kontağını çevirmek veya bir binanın temelini atmak gibidir.
 * Tarayıcı index.html dosyasını yükledikten sonra JavaScript motoru bu dosyayı çalıştırır.
 * =============================================================================
 */

// 1. İÇE AKTARMALAR (IMPORTS):
// Başka dosyalarda veya kütüphanelerde yazılmış hazır kod bloklarını bu dosyada kullanabilmek için 'import' ederiz.

// '@angular/platform-browser' paketinden 'bootstrapApplication' fonksiyonunu alıyoruz.
// "bootstrap" (başlatma/ateşleme) terimi, uygulamayı tarayıcı ortamında hayata geçiren fonksiyondur.
import { bootstrapApplication } from '@angular/platform-browser';

// Kendi oluşturduğumuz app.config.ts dosyasından genel uygulama yapılandırmalarını (servisler, ayarlar) içeri alıyoruz.
import { appConfig } from './app/app.config';

// Uygulamamızın ana çatısını oluşturan 'App' bileşenini (Component) içeri alıyoruz.
import { App } from './app/app';

/**
 * 2. UYGULAMAYI BAŞLATMA (BOOTSTRAP):
 * 
 * bootstrapApplication fonksiyonu iki önemli parametre alır:
 * 1. Parametre (App): Başlatılacak ana bileşen. Angular bu bileşeni index.html içindeki <app-root></app-root> alanına yerleştirir.
 * 2. Parametre (appConfig): Uygulama genelinde geçerli olacak kurallar, HTTP istek yetenekleri ve ayarlar listesi.
 * 
 * .catch((err) => console.error(err)):
 * Eğer uygulama başlarken beklenmedik bir hata meydana gelirse (örneğin bir dosya eksikse veya kodda yazım hatası varsa),
 * programın çöküp sessizce kalmasını engeller; hatayı yakalar (catch) ve geliştirici konsoluna (F12 Developer Tools)
 * kırmızı bir hata mesajı olarak yazdırır (console.error).
 */
bootstrapApplication(App, appConfig)
  .catch((err) => console.error(err));
