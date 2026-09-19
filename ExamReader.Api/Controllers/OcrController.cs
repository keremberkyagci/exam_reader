// =============================================================================
// KÜTÜPHANELER VE BAĞIMLILIKLAR (USING DİREKTİFLERİ)
// =============================================================================

// OCR tarama ve metin çıkarma servisini (IOcrService) kullanabilmek için:
using ExamReader.Api.Services;

// Web API denetleyicileri, HTTP özellikleri ve dosya yükleme araçları (IFormFile) için:
using Microsoft.AspNetCore.Mvc;

// =============================================================================
// AD ALANI (NAMESPACE) TANIMI
// =============================================================================
// Bu dosyanın "ExamReader.Api.Controllers" mantıksal klasörüne ait olduğunu belirtir.
namespace ExamReader.Api.Controllers;

// =============================================================================
// OCR DENETLEYİCİSİ (OCR CONTROLLER)
// =============================================================================
// "[ApiController]":
// Bu sınıfın bir REST API denetleyicisi olduğunu belirtir. Gelen istekleri dinler ve
// sonuçları otomatik olarak JSON formatında döner.
[ApiController]

// "[Route("api/ocr")]":
// Bu denetleyiciye internet üzerinden hangi adres yoluyla ulaşılacağını belirtir.
// Örnek: "http://localhost:5000/api/ocr" adresine gelen istekler bu sınıfa yönlendirilir.
[Route("api/ocr")]

// "public class OcrController : ControllerBase":
// ASP.NET Core'un temel kontrolcü yapısını (ControllerBase) miras alarak web isteklerini
// yönetmemizi sağlar.
public class OcrController : ControllerBase
{
    // =========================================================================
    // BAĞIMLILIK (SERVICE) DEĞİŞKENİ
    // =========================================================================
    // "IOcrService": Sınav kağıdı fotoğraflarını tarayan, resimdeki yazıları ve puanları
    // yapay zeka veya görüntü işleme teknikleriyle okuyan asıl servisimizin arayüzüdür.
    // "_ocrService": Bu servise erişmek için kullandığımız sınıf içi özel değişkenimizdir.
    private readonly IOcrService _ocrService;

    // =========================================================================
    // YAPICI METOT (CONSTRUCTOR) - DEPENDENCY INJECTION
    // =========================================================================
    // Sınıf ilk yaratıldığında ASP.NET Core tarafından çalıştırılır ve gerekli olan
    // IOcrService nesnesi buraya otomatik olarak enjekte edilir (bağlanır).
    public OcrController(IOcrService ocrService)
    {
        _ocrService = ocrService;
    }

    // =========================================================================
    // RESİM TARAMA VE METİN/NOT ÇIKARMA METODU (HTTP POST)
    // =========================================================================
    // Adres: POST api/ocr
    // Ne İşe Yarar? Kullanıcının yüklediği sınav kağıdı fotoğrafını alır, OCR ile tarar,
    // öğrenci adı ve puanı çıkarıp sonuç olarak döner.
    //
    // Neden POST?
    // Sunucuya bir dosya (fotoğraf) yükleyeceğimiz ve bir işlem başlatacağımız için
    // HTTP POST metodu kullanılır.
    //
    // Format: "multipart/form-data"
    // Dosya (resim/fotoğraf) yükleme istekleri standart metin gibi değil,
    // "multipart/form-data" (çok parçalı form verisi) formatında gönderilir.
    [HttpPost]

    // "[RequestSizeLimit(...)]" - YÜKLENEBİLECEK DOSYA BOYUTU SINIRI:
    // 10 * 1024 * 1024 bayt = 10 Megabayt (MB).
    // Bilgisayarda veri birimleri:
    //   - 1024 Bayt = 1 Kilobayt (KB)
    //   - 1024 Kilobayt = 1 Megabayt (MB)
    //   - 10 * 1024 * 1024 = 10 MB.
    // Neden Sınır Koyuyoruz? Kötü niyetli kişilerin sunucuya devasa boyutlarda dosyalar yükleyip
    // sunucunun belleğini (RAM) tüketerek sistemi çökertmesini engellemek için güvenlik önlemidir.
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB

    // "public async Task<IActionResult> Scan([FromForm] IFormFile image)"
    // - "async Task<IActionResult>": OCR işlemi resim analiz ettiği için birkaç saniye sürebilir.
    //   Asenkron yapı sayesinde sunucu bu sırada kilitlenmez, diğer isteklere de hizmet verir.
    // - "[FromForm]": Dosyanın bir form yüklemesi ile geldiğini belirtir.
    // - "IFormFile image": Kullanıcının yüklediği dosyanın (resmin) kendisini temsil eder.
    //   Dosyanın adına, uzantısına ve içerisindeki baytlara bu değişken üzerinden ulaşılır.
    public async Task<IActionResult> Scan([FromForm] IFormFile image)
    {
        // ---------------------------------------------------------------------
        // 1. ADIM: DOSYA GÖNDERİLMİŞ Mİ KONTROLÜ
        // ---------------------------------------------------------------------
        // "image is null": Hiç dosya seçilmemişse.
        // "image.Length == 0": Seçilen dosyanın içi tamamen boşsa (0 bayt ise).
        if (image is null || image.Length == 0)
            // Kullanıcıya HTTP 400 Bad Request (Geçersiz İstek) ve hata mesajı döner.
            return BadRequest("Görüntü dosyası gereklidir.");

        // ---------------------------------------------------------------------
        // 2. ADIM: HATA YAKALAMA BLOĞU (TRY - CATCH)
        // ---------------------------------------------------------------------
        // Bir akrobatın altındaki güvenlik ağı gibidir. Resim işleme sırasında beklenmeyen
        // bir aksilik çıkarsa (resim bozuksa, OCR motoru hata verirse vb.) tüm uygulamanın
        // çökmesini engeller ve hatayı nazikçe karşılar.
        try
        {
            // "try" Bloğu: "Bu işlemleri yapmayı dene" anlamına gelir.

            // OCR servisimizi çağırarak resmi taramasını ve içindeki bilgileri çıkarmasını istiyoruz.
            // "await": OCR motoru resmi okuyup bitirene kadar asenkron olarak bekler.
            var result = await _ocrService.ExtractAsync(image);

            // HTTP 200 OK (Başarılı) durum koduyla birlikte okunan sonuçları
            // (öğrenci adı, puan, ham metin) kullanıcıya JSON olarak döndürür.
            return Ok(result);
        }
        catch (Exception ex)
        {
            // "catch" Bloğu: "Yukarıdaki 'try' içinde herhangi bir hata meydana gelirse buraya geç."
            // "Exception ex": Meydana gelen hatanın tüm bilgilerini (mesajını, hangi satırda olduğunu) tutar.

            // HTTP 500 (Internal Server Error / Sunucu Hatası) kodu ile birlikte
            // hatanın sebebini JSON formatında istemciye döner.
            return StatusCode(500, new { error = ex.Message, details = ex.ToString() });
        }
    }
}
