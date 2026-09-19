// =============================================================================
// KÜTÜPHANELER VE BAĞIMLILIKLAR (USING DİREKTİFLERİ)
// =============================================================================
// Gerekli sınıfları ve araçları bu dosyaya çağırıyoruz.

// Sınav modelimizi (Exam) kullanabilmek için:
using ExamReader.Api.Models;

// Sınavlarla ilgili iş mantığını ve veritabanı işlemlerini yürüten servisi (IExamService) kullanmak için:
using ExamReader.Api.Services;

// ASP.NET Core Web API altyapı özelliklerini (ControllerBase, HttpGet vb.) kullanabilmek için:
using Microsoft.AspNetCore.Mvc;

// =============================================================================
// AD ALANI (NAMESPACE) TANIMI
// =============================================================================
// Bu sınıfın "ExamReader.Api.Controllers" mantıksal klasöründe yer aldığını belirtir.
namespace ExamReader.Api.Controllers;

// =============================================================================
// SINAVLAR DENETLEYİCİSİ (EXAMS CONTROLLER)
// =============================================================================
// "[ApiController]":
// Bu sınıfın bir REST API denetleyicisi olduğunu belirtir. İstekleri karşılar ve otomatik
// olarak veri dönüşümlerini ve hata kontrollerini gerçekleştirir.
[ApiController]

// "[Route("api/[controller]")]":
// Denetleyicinin internet adresindeki yolunu (URL Route) belirler.
// Buradaki "[controller]" özel bir yer tutucudur (placeholder). Sınıfın adındaki "Controller"
// kelimesini atarak geriye kalan kısmı rota yapar: "ExamsController" -> "api/exams" olur.
// Yani bu sınıfa "http://localhost:5000/api/exams" adresinden ulaşılır.
[Route("api/[controller]")]

// "public class ExamsController : ControllerBase":
// - "public": Sınıfın her yerden erişilebilir olmasını sağlar.
// - ": ControllerBase": ASP.NET Core'un temel denetleyici sınıfından miras alır.
//   Bu sayede Ok(), NotFound() gibi HTTP durum kodlarını üreten metotlara doğrudan erişebiliriz.
public class ExamsController : ControllerBase
{
    // =========================================================================
    // BAĞIMLILIK (SERVICE) DEĞİŞKENİ
    // =========================================================================
    // "private": Yalnızca bu sınıfın içinden erişilebilir.
    // "readonly": Sadece sınıf ilk oluşturulduğunda (yapıcı metotta) atanabilir, sonradan değiştirilemez.
    // "IExamService": Sınav verilerini veritabanından getiren servisin arayüzüdür (sözleşmesidir).
    // "_service": Servis nesnesini sakladığımız değişken.
    private readonly IExamService _service;

    // =========================================================================
    // YAPICI METOT (CONSTRUCTOR) - DEPENDENCY INJECTION (BAĞIMLILIK ENJEKSİYONU)
    // =========================================================================
    // Bu metot, sınıf belleğe yüklendiğinde otomatik olarak ilk çalışan başlangıç kodudur.
    // ASP.NET Core, sistemde kayıtlı olan IExamService nesnesini bulur ve buraya otomatik verir.
    public ExamsController(IExamService service)
    {
        // Gelen hazır servisi sınıfın içindeki değişkenimize kaydediyoruz.
        _service = service;
    }

    // =========================================================================
    // TÜM SINAVLARI LİSTELEME METODU (HTTP GET)
    // =========================================================================
    // Adres: GET api/exams
    // Ne İşe Yarar? Sistemdeki kayıtlı olan bütün sınavları (adları, oluşturulma tarihleri vb.) getirir.
    //
    // "[HttpGet]": Metodun HTTP GET isteğiyle çalışacağını ifade eder (sadece veri okur).
    // "async Task<ActionResult<IEnumerable<Exam>>> GetAll()":
    // - "async" / "Task": Asenkron çalışır, veritabanı cevabı gelene kadar sistemi dondurmaz.
    // - "ActionResult<T>": Sonuç olarak hem HTTP durum kodunu hem de dönen veriyi paketler.
    // - "IEnumerable<Exam>": Birden çok Exam (Sınav) nesnesinden oluşan bir listeyi temsil eder.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Exam>>> GetAll()
    {
        // Servisten tüm sınavları asenkron olarak talep ediyoruz.
        var exams = await _service.GetAllExamsAsync();

        // HTTP 200 OK kodu ve sınav listesi ile birlikte kullanıcıya yanıt dönüyoruz.
        return Ok(exams);
    }

    // =========================================================================
    // ID İLE TEK BİR SINAV GETİRME METODU (HTTP GET)
    // =========================================================================
    // Adres: GET api/exams/{id} (Örneğin: api/exams/1 veya api/exams/42)
    // Ne İşe Yarar? Verilen Id numarasına sahip olan tek bir sınavın detaylarını getirir.
    //
    // "[HttpGet("{id:int}")]":
    // - "{id}": URL adresindeki dinamik sayıyı yakalar (Örn: api/exams/5 adresindeki 5 sayısını alır).
    // - ":int": Rota kısıtlamasıdır (Route Constraint). URL'deki bu değerin mutlaka bir tam sayı
    //   olması gerektiğini şart koşar. Eğer harf girilirse bu metot hiç çalışmaz.
    //
    // "int id": URL'den yakalanan kimlik numarasının metot içerisindeki parametre halidir.
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Exam>> GetById(int id)
    {
        // Verilen Id numarasına göre servisten sınav bilgisini çekiyoruz.
        var exam = await _service.GetExamByIdAsync(id);

        // "null" Kontrolü:
        // "null", programlamada "hiçbir şey yok", "boşluk" veya "aranan şey bulunamadı" demektir.
        // Eğer veritabanında bu Id numarasına sahip bir sınav yoksa:
        if (exam is null)
            // HTTP 404 Not Found (Kaynak Bulunamadı) hata kodunu döndürür.
            return NotFound();

        // Eğer sınav bulunduysa HTTP 200 OK kodu ile birlikte sınav nesnesini döner.
        return Ok(exam);
    }
}
