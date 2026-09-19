// ==============================================================================================
// DOSYA ADI: ExamService.cs
// AÇIKLAMA: Bu dosya, sınavlar ile ilgili iş kurallarını ve en önemlisi "Önbellekleme" (Caching)
// mekanizmasını yöneten servis katmanıdır.
// Veri tabanını yormamak ve uygulamayı süper hızlı hale getirmek için Redis kütüphanesini kullanır.
// ==============================================================================================

// ----------------------------------------------------------------------------------------------
// "using" İFADELERİ
// ----------------------------------------------------------------------------------------------

// Sınav veri modelini kullanabilmek için içeri aktarır.
using ExamReader.Api.Models;

// Sınavların veri tabanı işlemlerini yapan IExamRepository arayüzünü içeri aktarır.
using ExamReader.Api.Repositories;

// Redis önbellek sunucusu ile iletişim kurmak için kullanılan kütüphane.
using StackExchange.Redis;

// C# nesnelerini metne (JSON), metinleri de C# nesnelerine çevirmek için kullanılan kütüphane.
using System.Text.Json;

namespace ExamReader.Api.Services;

// ----------------------------------------------------------------------------------------------
// INTERFACE (ARAYÜZ) - IExamService
// Dış dünyanın (Controller'ların) bu servisten talep edebileceği işlerin listesi.
// ----------------------------------------------------------------------------------------------
public interface IExamService
{
    // Tüm sınavları liste olarak getiren fonksiyon sözleşmesi.
    Task<IEnumerable<Exam>> GetAllExamsAsync();

    // Belirli bir ID'ye sahip sınavı getiren fonksiyon sözleşmesi (Bulunamazsa null dönebilir).
    Task<Exam?> GetExamByIdAsync(int id);
}

// ----------------------------------------------------------------------------------------------
// SOMUT SINIF (CLASS) - ExamService
// Sınav servisi mantığını ve Redis önbellek algoritmasını yürüten ana sınıf.
// ----------------------------------------------------------------------------------------------
public class ExamService : IExamService
{
    // Veri tabanına sorgu atabilmek için kullanacağımız depo (Repository) referansı.
    private readonly IExamRepository _repository;

    // Redis sunucusundaki veri tabanına erişmek ve hızlı veri okuyup yazmak için kullanılan araç.
    private readonly IDatabase _redis;

    // ------------------------------------------------------------------------------------------
    // CONSTRUCTOR (YAPICI METOT)
    // Sınıf ilk yaratıldığında Dependency Injection (Bağımlılık Enjeksiyonu) ile:
    // 1. Sınav deposu (repository)
    // 2. Redis bağlantısı (redis)
    // sisteme otomatik olarak verilir.
    // ------------------------------------------------------------------------------------------
    public ExamService(IExamRepository repository, IConnectionMultiplexer redis)
    {
        _repository = repository;
        // Redis bağlantı havuzundan varsayılan veri tabanını alıp _redis değişkenine atıyoruz:
        _redis = redis.GetDatabase();
    }

    // ------------------------------------------------------------------------------------------
    // METOT: GetAllExamsAsync
    // Tüm sınavları doğrudan veri tabanından getirir.
    // ------------------------------------------------------------------------------------------
    public async Task<IEnumerable<Exam>> GetAllExamsAsync()
    {
        return await _repository.GetAllAsync();
    }

    // ------------------------------------------------------------------------------------------
    // METOT: GetExamByIdAsync (ÖNBELLEK - CACHING STRATEJİSİ)
    // Bu metot "Cache-Aside" (Önbelleğe Bak, Yoksa Veri Tabanına Git) mantığıyla çalışır:
    //
    // Günlük Hayattan Benzetme:
    // Sürekli aradığınız bir bilgiyi çalışma masanızın üzerindeki yapışkan nota (Redis/RAM) yazarsınız.
    // Birisi size sorduğunda önce masaya bakarsınız (çok hızlı). Masada yoksa arşiv odasına (PostgreSQL)
    // gider, dosyayı bulur, masanıza da bir kopyasını yapıştırır, sonra cevabı verirsiniz.
    // ------------------------------------------------------------------------------------------
    public async Task<Exam?> GetExamByIdAsync(int id)
    {
        // 1. ADIM: Redis için benzersiz bir anahtar (Key) ismi oluşturuyoruz.
        // Örnek: id'si 5 olan sınav için bu anahtar "exam:5" olacaktır.
        string cacheKey = $"exam:{id}";

        // 2. ADIM: Bu anahtarla Redis önbelleğinde bir kayıt var mı diye soruyoruz (Çok hızlıdır).
        var cached = await _redis.StringGetAsync(cacheKey);

        // 3. ADIM: Eğer Redis'te bu bilgi varsa (Cache Hit):
        if (cached.HasValue)
        {
            // Redis'te veriler yazı (JSON metni) olarak saklanır.
            // JsonSerializer.Deserialize: Metin halindeki JSON verisini tekrar C# 'Exam' nesnesine dönüştürür.
            // Hiç PostgreSQL veri tabanına gitmeden hemen sonucu dönerek zamandan ve performanstan büyük tasarruf sağlarız!
            return JsonSerializer.Deserialize<Exam>((string)cached!);
        }

        // 4. ADIM: Eğer Redis'te yoksa (Cache Miss):
        // Mecburen ana veri tabanımıza (PostgreSQL) gidip sınavı sorguluyoruz.
        var exam = await _repository.GetByIdAsync(id);

        // 5. ADIM: Eğer sınav veri tabanında bulunduysa:
        if (exam is not null)
        {
            // Bir dahaki sefere veri tabanına gitmeye gerek kalmasın diye sonucu Redis'e yazıyoruz.
            // - JsonSerializer.Serialize: C# nesnesini metne (JSON formatına) dönüştürür.
            // - TimeSpan.FromMinutes(5): "Bu veri önbellekte 5 dakika dursun, sonra otomatik silinsin" (TTL - Time to Live).
            //   Böylece belleğimiz gereksiz şişmez ve veriler eskiyip bayatlamaz.
            await _redis.StringSetAsync(cacheKey, JsonSerializer.Serialize(exam), TimeSpan.FromMinutes(5));
        }

        // 6. ADIM: Bulunan sınavı (veya bulunamadıysa null değerini) geri döndürürüz.
        return exam;
    }
}
