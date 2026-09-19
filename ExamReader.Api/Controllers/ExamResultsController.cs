// =============================================================================
// KÜTÜPHANELER VE BAĞIMLILIKLAR (USING DİREKTİFLERİ)
// =============================================================================
// "using", başka klasörlerde veya harici kütüphanelerde yazılmış hazır kodları
// bu dosya içerisinde doğrudan kullanabilmemizi sağlayan bir içeri aktarma komutudur.

// Modellerimizi (ExamResult, CreateExamResultRequest vb.) kullanabilmek için:
using ExamReader.Api.Models;

// Veritabanı ve iş mantığını yürüten servisleri (IExamResultService vb.) kullanabilmek için:
using ExamReader.Api.Services;

// Web API oluşturmak için gerekli olan ASP.NET Core araçlarını (ControllerBase, HttpGet vb.) kullanabilmek için:
using Microsoft.AspNetCore.Mvc;

// =============================================================================
// AD ALANI (NAMESPACE) TANIMI
// =============================================================================
// Bu dosyanın, projenin "Controllers" (Denetleyiciler) katmanına ait olduğunu belirtir.
namespace ExamReader.Api.Controllers;

// =============================================================================
// DENETLEYİCİ SINIFI (CONTROLLER CLASS)
// =============================================================================
// "CONTROLLER" NEDİR?
// Bir restorandaki "garson" gibidir. Müşteri (mobil uygulama veya internet sitesi)
// bir istekte bulunur (sipariş verir). Controller bu isteği karşılar, mutfağa (servis katmanına)
// bildirir ve mutfaktan çıkan yemeği (veriyi) müşteriye uygun bir formatta geri sunar.

// "[ApiController]":
// ASP.NET Core'a bu sınıfın bir Web API denetleyicisi olduğunu bildirir.
// Gelen verilerin otomatik kontrol edilmesi ve hatalı isteklerde anlaşılır mesajlar
// üretilmesi gibi birçok modern kolaylığı arka planda bizim yerimize otomatik sağlar.
[ApiController]

// "[Route("api/exam-results")]":
// Bu denetleyiciye internet üzerinden hangi adres (URL) yoluyla ulaşılacağını belirler.
// Örneğin: "http://localhost:5000/api/exam-results" adresine istek geldiğinde bu sınıf çalışır.
[Route("api/exam-results")]

// "public class ExamResultsController : ControllerBase":
// - "public": Sınıfın dışarıdan erişilebilir olduğunu belirtir.
// - ": ControllerBase": ASP.NET Core'un hazır Controller özelliklerini miras alır.
//   Böylece "Ok()", "BadRequest()" gibi hazır HTTP cevap fonksiyonlarını kolayca kullanabiliriz.
public class ExamResultsController : ControllerBase
{
    // =========================================================================
    // BAĞIMLILIK (SERVICE) TANIMI
    // =========================================================================
    // "private": Bu değişkene sadece bu sınıfın içinden erişilebilir, dışarıdan değiştirilemez.
    // "readonly": Sadece sınıf ilk kurulurken (constructor içinde) değeri verilebilir,
    // sonradan kaza eseri değeri bozulamaz.
    // "IExamResultService": Sınav sonuçlarını veritabanına kaydeden ve oradan okuyan
    // iş mantığı servisimizin kurallarını belirleyen arayüzdür (Interface).
    // Controller doğrudan veritabanıyla uğraşmaz, işi bu servise devreder.
    // "_service": Servis nesnemizi tuttuğumuz değişkenimizdir (başına alt çizgi konması C# geleneğidir).
    private readonly IExamResultService _service;

    // =========================================================================
    // YAPICI METOT (CONSTRUCTOR) VE BAĞIMLILIK ENJEKSİYONU (DEPENDENCY INJECTION)
    // =========================================================================
    // Bu metot, bu sınıftan yeni bir örnek oluşturulduğunda İLK VE OTOMATİK çalışan özel bir metottur.
    // "service": ASP.NET Core sistemi, ihtiyacımız olan servisi arka planda hazırlar ve
    // buraya parametre olarak otomatik verir (Buna "Dependency Injection" denir).
    public ExamResultsController(IExamResultService service)
    {
        // Dışarıdan gelen hazır servisi, sınıfımızın içindeki özel değişkene kaydediyoruz.
        _service = service;
    }

    // =========================================================================
    // TÜM SINAV SONUÇLARINI GETİRME METODU (HTTP GET)
    // =========================================================================
    // Adres: GET api/exam-results
    // Ne İşe Yarar? Sistemde kayıtlı olan bütün öğrenci sınav sonuçlarını listeler.
    //
    // "[HttpGet]":
    // Bu metodun bir HTTP GET isteği (veri okuma isteği) geldiğinde çalışacağını belirtir.
    //
    // "async Task<ActionResult<IEnumerable<ExamResult>>> GetAll()":
    // - "async" ve "Task": Asenkron programlama sağlar. Veritabanından verilerin gelmesi
    //   biraz zaman alabilir. Sistem beklerken sunucu kilitlenmez, diğer kullanıcıların
    //   isteklerine cevap vermeye devam edebilir.
    // - "ActionResult<T>": Metodun geriye hem veriyi (T) hem de bir HTTP durum kodunu
    //   (örneğin 200 OK başarı kodu) birlikte döndüreceğini belirtir.
    // - "IEnumerable<ExamResult>": Birden çok ExamResult nesnesini barındıran bir liste/koleksiyondur.
    // - "GetAll": Metodun adıdır ("Hepsini Getir").
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExamResult>>> GetAll()
    {
        // Servisimize gidip "bütün sınav sonuçlarını getir" diyoruz.
        // "await": Veritabanından cevap gelene kadar işlemi asenkron olarak bekletir.
        var results = await _service.GetAllAsync();

        // "Ok(...)": HTTP 200 OK (İşlem Başarılı) koduyla beraber bulunan sonuçları
        // JSON formatına dönüştürüp kullanıcıya teslim eder.
        return Ok(results);
    }

    // =========================================================================
    // YENİ SINAV SONUCU EKLEME METODU (HTTP POST)
    // =========================================================================
    // Adres: POST api/exam-results
    // Ne İşe Yarar? Yeni bir sınav sonucunu sisteme kaydeder.
    // Gönderilecek Örnek Veri (JSON Body):
    // { "studentName": "Ali İhsan", "score": 80 }
    //
    // "[HttpPost]":
    // Sunucuya yeni bir veri göndermek/kaydetmek için kullanılan HTTP POST isteğini temsil eder.
    //
    // "[FromBody] CreateExamResultRequest request":
    // İstemciden (tarayıcıdan veya mobil uygulamadan) gönderilen JSON paketini alır,
    // otomatik olarak "CreateExamResultRequest" nesnemize dönüştürür.
    //
    // "IActionResult":
    // Geriye Ok(), BadRequest() gibi standart bir HTTP yanıtı döneceğini belirtir.
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateExamResultRequest request)
    {
        // ---------------------------------------------------------------------
        // 1. ADIM: VERİ DOĞRULAMA (VALIDATION)
        // ---------------------------------------------------------------------
        // Sınav notu mantıken 0 ile 100 arasında olmalıdır.
        // Eğer 0'dan küçük veya 100'den büyük bir puan girilmişse hata veriyoruz.
        if (request.Score < 0 || request.Score > 100)
            // "BadRequest(...)": HTTP 400 (Geçersiz/Hatalı İstek) kodunu ve açıklama mesajını döner.
            return BadRequest("Score 0 ile 100 arasında olmalıdır.");

        // ---------------------------------------------------------------------
        // 2. ADIM: YENİ VERİTABANI NESNESİNİ HAZIRLAMA
        // ---------------------------------------------------------------------
        // Kullanıcının gönderdiği istekten (request) verileri alıp asıl veritabanı
        // modelimiz olan "ExamResult" nesnesine aktarıyoruz.
        var examResult = new ExamResult
        {
            StudentName = request.StudentName, // İstekten gelen öğrenci adı
            Score       = request.Score        // İstekten gelen sınav puanı
        };

        // ---------------------------------------------------------------------
        // 3. ADIM: VERİTABANINA KAYDETME
        // ---------------------------------------------------------------------
        // Servisimizi çağırarak bu yeni kaydı veritabanına asenkron olarak yazdırıyoruz.
        await _service.AddAsync(examResult);

        // ---------------------------------------------------------------------
        // 4. ADIM: BAŞARILI YANIT DÖNME
        // ---------------------------------------------------------------------
        // "Ok(examResult)": HTTP 200 OK koduyla birlikte yeni oluşturulan ve veritabanına
        // kaydedilen sınav sonucunu kullanıcıya geri gönderir.
        return Ok(examResult);
    }
}
