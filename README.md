# 📝 Exam Reader OCR

**Exam Reader OCR**, öğretmenlerin sınav kağıtlarındaki öğrenci adı ve not bilgisini telefon veya bilgisayar kamerasıyla okutup saniyeler içinde doğrudan PostgreSQL veritabanına kaydetmesini sağlayan tam yığın (Full-Stack) bir uygulamadır.

Modern arayüzü, otomatik görüntü küçültme sistemi ve Tesseract OCR motoru sayesinde yüksek hızlı ve hata payı düşük bir deneyim sunar.

![Angular](https://img.shields.io/badge/Frontend-Angular_18-dd0031?style=for-the-badge&logo=angular)
![.NET Core](https://img.shields.io/badge/Backend-.NET_8.0_API-512BD4?style=for-the-badge&logo=dotnet)
![PostgreSQL](https://img.shields.io/badge/Database-Supabase%20PostgreSQL-336791?style=for-the-badge&logo=postgresql)
![Tesseract](https://img.shields.io/badge/OCR-Tesseract_5-brightgreen?style=for-the-badge)

---

## ✨ Özellikler

- **📷 Kamera ve Dosya Desteği:** İster canlı olarak kağıdın fotoğrafını çekin, ister galerinizden yükleyin.
- **⚡ Hızlı Okuma:** Yüklenen fotoğraflar tarayıcıda anında `1200px` boyutuna küçültülerek arka yüze (API) aktarılır. Bu sayede OCR süresi minimuma iner.
- **✏️ Manuel Ekleme:** OCR kullanmak istemediğiniz anlar için pratik manuel not ekleme arayüzü.
- **🔔 Gelişmiş Bildirimler:** İşlem başarıları ve okuma hataları için anlık, şık bildirimler (Toast).
- **🗄️ Veritabanı (Dapper):** Güçlü ve çok hızlı Dapper ORM kullanılarak veriler Supabase PostgreSQL veritabanına yazılır.

---

## 🛠️ Kurulum (Yerel Ortam)

Projeyi bilgisayarınızda çalıştırmak için aşağıdaki adımları izleyin.

### Ön Koşullar
- Node.js (v18+) ve Angular CLI
- .NET 8 SDK
- PostgreSQL (Local veya Supabase)

### 1. Veritabanı Ayarları
1. Ana dizinde bulunan `database.sql` dosyasındaki kodları Supabase SQL editörünüzde veya yerel PostgreSQL veritabanınızda çalıştırın.
2. `ExamReader.Api/appsettings.json` ve `appsettings.Development.json` içerisindeki `PostgreSql` bağlantı dizelerini (Connection String) kendi veritabanınıza göre güncelleyin.

### 2. Uygulamayı Başlatma (Tek Tıkla)
Proje dizininde yer alan `baslat.bat` dosyasına çift tıklayarak hem Backend API'yi hem de Angular Web uygulamasını tek bir tıkla başlatabilirsiniz. 

Alternatif olarak terminallerden manuel başlatmak için:
- **Backend:** `cd ExamReader.Api` -> `dotnet run`
- **Frontend:** `cd exam-reader-web` -> `ng serve`

Tarayıcıda [http://localhost:4200](http://localhost:4200) adresine giderek uygulamayı kullanabilirsiniz.

---

## 🚀 Canlıya Alma (Deployment) Rehberi

Bu proje tamamen ücretsiz servisler (Render.com + GitHub Pages + Supabase) ile canlıya alınacak şekilde tasarlanmıştır.

### 1. Veritabanı: Supabase
- Supabase.com üzerinden ücretsiz PostgreSQL projesi oluşturun.
- `database.sql` dosyasını çalıştırın ve size verilen **Connection String** bilgisini kaydedin.

### 2. Backend (API): Render.com
API içerisinde Tesseract OCR bulunduğundan özel Linux bağımlılıklarına ihtiyaç duyar.
- `ExamReader.Api/Dockerfile` dosyası bu sistem için özel olarak hazırlanmıştır.
- Render.com'da **Web Service** oluşturup, Docker ortamını seçerek deponuzu bağlayın.
- `appsettings.json` içindeki bağlantı dizesini Supabase adresinizle güncellemeyi unutmayın.

### 3. Frontend (Web): GitHub Pages
- Angular kodunuzdaki `src/app/exam-result.service.ts` dosyasında yer alan `http://localhost:5197` API adresini, Render.com'un size verdiği canlı URL ile değiştirin.
- Terminalde `ng build --configuration production --base-href /repo-adi/` komutunu çalıştırın.
- Oluşan `dist/` klasörünün içindekileri GitHub reponuzda `gh-pages` dalına (branch) aktarın.

---

## 📝 OCR Kullanım Kuralları
- **El yazısı geçersizdir.** Lütfen büyük ve ayrık (BLOK) harflerle yazın.
- Kağıttaki format tam olarak şöyle olmalıdır:
  ```text
  öğrenci adı : ALI IHSAN
  öğrenci notu : 80
  ```
- Fotoğrafı iyi ışıkta, gölgesiz ve net çekin. (Word dosyasından ekran görüntüsü almak da iyi bir test yöntemidir).

---

> Bu proje, **Google Antigravity AI** asistanı ile çift programlama (pair-programming) yöntemiyle geliştirilmiştir. 🛸
