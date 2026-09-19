// ==============================================================================================
// DOSYA ADI: Program.cs
// AÇIKLAMA: Bu dosya, tüm web API uygulamamızın başlangıç noktasıdır (yani giriş kapısıdır).
// Program ilk çalıştırıldığında bilgisayar doğrudan bu dosyanın en üstünden okumaya başlar.
// Burada uygulamamızın ihtiyaç duyduğu ayarlar, veri tabanı bağlantıları, güvenlik kuralları
// tanımlanır ve en sonunda web sunucusu çalıştırılarak istekleri dinlemeye başlar.
// ==============================================================================================

// ----------------------------------------------------------------------------------------------
// "using" İFADELERİ (KÜTÜPHANE VE MODÜL ÇAĞIRMA)
// Programlama dillerinde her şeyi sıfırdan yazmayız. Başka dosyalarda ya da hazır kütüphanelerde
// yazılmış olan hazır kod bloklarını bu dosyaya dahil etmek için "using" anahtar kelimesini kullanırız.
// Bir nevi marangozun işe başlamadan önce takım çantasından gerekli aletleri masaya dizmesi gibidir.
// ----------------------------------------------------------------------------------------------

// Kendi projemiz içindeki veri tabanı sorgu sınıflarını (Repositories) içeri aktarır.
using ExamReader.Api.Repositories;

// Kendi projemiz içindeki iş mantığını (Business Logic) yürüten servisleri içeri aktarır.
using ExamReader.Api.Services;

// PostgreSQL veri tabanına bağlanabilmemizi sağlayan hazır kütüphaneyi içeri aktarır.
using Npgsql;

// Redis adındaki çok hızlı çalışan önbellek (hafıza) sistemine bağlanmayı sağlayan kütüphane.
using StackExchange.Redis;

// Veri tabanı bağlantıları için temel standart arayüzleri (IDbConnection gibi) içeren kütüphane.
using System.Data;

// .env isimli gizli ayar dosyasındaki değişkenleri (şifreler, bağlantı adresleri) okuyan kütüphane.
using DotNetEnv;

// ----------------------------------------------------------------------------------------------
// 1. ADIM: ÇEVRESEL DEĞİŞKENLERİ (.env DOSYASINI) YÜKLEME
// Gizli şifreler, veri tabanı adresleri gibi bilgileri doğrudan kod içine yazmak güvenlik açığıdır.
// Bu yüzden bu bilgiler ".env" dosyasında tutulur. Aşağıdaki satır o dosyayı okur ve sisteme yükler.
// ----------------------------------------------------------------------------------------------
Env.Load();

// ----------------------------------------------------------------------------------------------
// 2. ADIM: UYGULAMA İNŞA EDİCİSİ (BUILDER) OLUŞTURMA
// "WebApplication.CreateBuilder" web uygulamamızı inşa edecek olan ana inşaat ustasıdır.
// Uygulamanın ayarlarını, servislerini ve kurallarını bu 'builder' nesnesi içerisine ekleyeceğiz.
// 'args' ise program başlatılırken konsoldan verilen özel komut parametreleridir.
// ----------------------------------------------------------------------------------------------
var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------------------------------------------------
// 3. ADIM: CORS (GÜVENLİK VE ERİŞİM İZNİ) AYARLARI
// CORS (Cross-Origin Resource Sharing): Tarayıcıların güvenlik mekanizmasıdır.
// Varsayılan olarak bir web sitesi (örneğin Angular ön yüzü), başka bir adresteki API'ye
// doğrudan istek atamaz (engellenir). Biz burada "Şu şu adreslerden gelen isteklere izin ver" diyoruz.
// ----------------------------------------------------------------------------------------------

// Ortam değişkenlerinden (Configuration) CORS için izin verilen adresi oku.
// Eğer böyle bir ayar bulunamazsa '??' (null birleştirme operatörü) devreye girer ve varsayılan olarak "http://localhost:4200" adresini alır.
var allowedCorsOrigin = builder.Configuration["ALLOWED_CORS_ORIGIN"] ?? "http://localhost:4200";

// CORS servisini uygulamaya tanıtıyoruz:
builder.Services.AddCors(options =>
{
    // "Angular" adında özel bir izin politikası (kural seti) oluşturuyoruz.
    options.AddPolicy("Angular", policy =>
        // WithOrigins: Bu API'ye hangi internet sitelerinin istek atabileceğini belirtiyoruz.
        policy.WithOrigins(
                allowedCorsOrigin, 
                "https://keremberkyagci.github.io", 
                "http://localhost:4200", 
                "https://keremberkyagciportfolio.me", 
                "http://keremberkyagciportfolio.me"
              )
              // Alt alan adlarına (subdomain) da izin verilmesini sağlar.
              .SetIsOriginAllowedToAllowWildcardSubdomains()
              // Her türlü başlık (header) bilgisine izin ver (Örn: Yetkilendirme tokenları).
              .AllowAnyHeader()
              // Her türlü HTTP metoduna izin ver (GET, POST, PUT, DELETE vb.).
              .AllowAnyMethod());
});

// ----------------------------------------------------------------------------------------------
// 4. ADIM: CONTROLLER (KONTROLCÜ) SERVİSLERİNİ EKLEME
// Controller'lar, internetten gelen istekleri (örneğin "bana sınavları getir" isteğini) karşılayan
// ve uygun cevabı hazırlayan sınıflardır. Bunları sisteme tanıtıyoruz.
// ----------------------------------------------------------------------------------------------
builder.Services.AddControllers();

// ----------------------------------------------------------------------------------------------
// 5. ADIM: BAĞIMLILIK ENJEKSİYONU (DEPENDENCY INJECTION) VE VERİ TABANI BAĞLANTILARI
// Dependency Injection (DI): Bir sınıfın çalışması için ihtiyaç duyduğu araçları (bağımlılıkları)
// sınıfın kendi içinde oluşturması yerine, sistemin dışarıdan otomatik olarak temin etmesidir.
// 
// Yaşam Süreleri (Lifetimes):
// - AddScoped: Gelen her kullanıcı isteği (HTTP Request) için yeni bir nesne oluşturur. İstek bitince nesne yok edilir.
// - AddSingleton: Uygulama çalıştığı sürece sadece TEK BİR nesne oluşturur ve herkes aynı nesneyi paylaşır.
// ----------------------------------------------------------------------------------------------

// PostgreSQL Veri Tabanı Bağlantısı (Dapper ile kullanılır):
// IDbConnection istendiğinde, appsettings.json veya .env içindeki "PostgreSql" bağlantı cümlesini
// kullanarak yeni bir NpgsqlConnection (PostgreSQL bağlantısı) oluşturur.
builder.Services.AddScoped<IDbConnection>(_ =>
    new NpgsqlConnection(builder.Configuration.GetConnectionString("PostgreSql")));

// Redis Önbellek Bağlantısı:
// Redis sunucusuna bağlantı ağır bir işlem olduğu için tek bir sefer bağlanılır (Singleton)
// ve tüm uygulama boyunca aynı açık bağlantı tekrar tekrar kullanılır.
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));

// Sınavlar ile ilgili Veri Tabanı Katmanı (Repository) ve İş Mantığı Katmanı (Service):
// Kodun içinde 'IExamRepository' arayüzü istendiğinde, somut 'ExamRepository' sınıfı verilsin.
builder.Services.AddScoped<IExamRepository, ExamRepository>();
// Kodun içinde 'IExamService' arayüzü istendiğinde, somut 'ExamService' sınıfı verilsin.
builder.Services.AddScoped<IExamService, ExamService>();

// Sınav Sonuçları ile ilgili Veri Tabanı Katmanı ve İş Mantığı Katmanı:
builder.Services.AddScoped<IExamResultRepository, ExamResultRepository>();
builder.Services.AddScoped<IExamResultService, ExamResultService>();

// OCR (Optik Karakter Tanıma - Resimdeki yazıları okuma) Servisi:
// Fotoğraflardan öğrenci adı ve notunu çıkaran servisimiz sisteme kaydediliyor.
builder.Services.AddScoped<IOcrService, OcrService>();

// ----------------------------------------------------------------------------------------------
// 6. ADIM: UYGULAMAYI İNŞA ETME (BUILD)
// Yukarıda tüm servisleri, ayarları ve kuralları builder nesnesine ekledik.
// Şimdi 'builder.Build()' diyerek çalışan hazır bir web uygulaması (app) meydana getiriyoruz.
// ----------------------------------------------------------------------------------------------
var app = builder.Build();

// ----------------------------------------------------------------------------------------------
// 7. ADIM: ARA YAZILIMLAR (MIDDLEWARE / BORU HATTI)
// Bir kullanıcıdan istek geldiğinde sırasıyla bu adımlardan geçer.
// ----------------------------------------------------------------------------------------------

// Yukarıda tanımladığımız "Angular" CORS kuralını devreye alıyoruz (güvenlik kontrolü).
app.UseCors("Angular");

// Yetkilendirme kontrolü (Gelen kullanıcının bu işlemi yapmaya yetkisi var mı?).
app.UseAuthorization();

// Gelen HTTP isteklerini (URL'leri) ilgili Controller fonksiyonlarına yönlendirir.
app.MapControllers();

// ----------------------------------------------------------------------------------------------
// 8. ADIM: UYGULAMAYI ÇALIŞTIRMA (RUN)
// Web sunucusunu başlatır. Artık sunucu 7/24 gelen bağlantıları dinler ve cevap verir.
// ----------------------------------------------------------------------------------------------
app.Run();
