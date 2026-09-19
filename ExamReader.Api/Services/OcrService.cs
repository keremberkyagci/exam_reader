// ==============================================================================================
// DOSYA ADI: OcrService.cs
// AÇIKLAMA: Bu dosya projenin en "akıllı" parçasıdır: OCR (Optik Karakter Tanıma) Servisi.
// Görevi: Kullanıcının yüklediği sınav kağıdı fotoğrafını alır, fotoğrafın içindeki yazıları
// bilgisayarın okuyabileceği dijital metne dönüştürür (Tesseract OCR motoru ile), ardından
// Regex (Düzenli İfadeler) kullanarak "Öğrenci Adı" ve "Sınav Notu" kısımlarını otomatik ayıklar.
// ==============================================================================================

// ----------------------------------------------------------------------------------------------
// "using" İFADELERİ
// ----------------------------------------------------------------------------------------------

// OcrResult modelini (ayrıştırılan öğrenci adı, notu ve ham metni tutan kalıp) içeri aktarır.
using ExamReader.Api.Models;

// Tesseract: Dünyaca ünlü açık kaynaklı optik karakter tanıma (OCR) kütüphanesidir.
using Tesseract;

// Regex (Regular Expressions): Metinler içinde kalıp arama, metin ayrıştırma işlemleri için standart C# kütüphanesi.
using System.Text.RegularExpressions;

namespace ExamReader.Api.Services;

// ----------------------------------------------------------------------------------------------
// INTERFACE (ARAYÜZ) - IOcrService
// OCR servisinin dışarıya sunduğu işlevin sözleşmesidir.
// ----------------------------------------------------------------------------------------------
public interface IOcrService
{
    // ExtractAsync: Kendisine gönderilen bir resim dosyasını (IFormFile) analiz edip,
    // sonucunda öğrenci adı ve notunu barındıran OcrResult nesnesini döndüreceğini taahhüt eder.
    Task<OcrResult> ExtractAsync(IFormFile image);
}

// ----------------------------------------------------------------------------------------------
// SOMUT SINIF (CLASS) - OcrService
// Resim işleme, Tesseract OCR motorunu çalıştırma ve metin ayrıştırma mantığını içeren ana sınıf.
// ----------------------------------------------------------------------------------------------
public class OcrService : IOcrService
{
    // Tesseract'ın Türkçe harfleri ve kelimeleri tanıması için ihtiyaç duyduğu
    // dil eğitim dosyalarının (tur.traineddata) bulunduğu klasör yolu.
    private readonly string _tessDataPath;

    // ------------------------------------------------------------------------------------------
    // CONSTRUCTOR (YAPICI METOT)
    // IWebHostEnvironment env: Uygulamanın çalıştığı kök dizin yolunu (ContentRootPath) öğrenmemizi sağlar.
    // ------------------------------------------------------------------------------------------
    public OcrService(IWebHostEnvironment env)
    {
        // Path.Combine: İşletim sistemine uygun şekilde (Windows'ta '\', Linux'ta '/') dosya yolunu birleştirir.
        // Projenin ana klasöründeki "tessdata" klasörünün tam disk yolunu elde ediyoruz.
        _tessDataPath = Path.Combine(env.ContentRootPath, "tessdata");
    }

    // ------------------------------------------------------------------------------------------
    // METOT: ExtractAsync
    // Kullanıcının yüklediği resmi diskte geçici olarak saklar, OCR motorundan geçirir ve temizlik yapar.
    // ------------------------------------------------------------------------------------------
    public async Task<OcrResult> ExtractAsync(IFormFile image)
    {
        // 1. ADIM: GEÇİCİ DOSYA OLUŞTURMA
        // Tesseract kütüphanesi resmi doğrudan diskteki bir dosya yolundan okumayı tercih eder.
        // - Path.GetTempPath(): Bilgisayarın geçici dosya klasörünü verir (Örn: C:\Users\...\AppData\Local\Temp).
        // - Guid.NewGuid(): Tamamen rastgele, eşsiz 32 karakterlik bir kimlik üretir. Aynı anda yüzlerce kişi
        //   resim yüklese bile dosya adları asla çakışmaz!
        // - Path.GetExtension(image.FileName): Yüklenen dosyanın uzantısını alır (.jpg, .png vb.).
        var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}");

        // Yüklenen resmin baytlarını diskte oluşturduğumuz geçici dosyaya yazıyoruz:
        // 'await using' ve 'stream': İşlem bittiğinde dosyanın bellekten ve disk kilidinden otomatik serbest bırakılmasını sağlar.
        await using (var stream = File.Create(tempPath))
            await image.CopyToAsync(stream);

        // 2. ADIM: RESMİ OKUMA VE HATA YÖNETİMİ (TRY - FINALLY)
        // finally bloğu sayesinde: Kod başarılı olsa da, hata verse de o geçici dosya diski doldurmasın diye mutlaka silinecektir!
        try
        {
            string rawText = string.Empty; // OCR motorunun resimde okuduğu ham (işlenmemiş) yazı burada tutulacak.

            try
            {
                // BİRİNCİ YOL: Doğrudan C# Tesseract Kütüphanesini Kullanma
                // - TesseractEngine: OCR motorunu başlatır. Dil olarak "tur" (Türkçe) seçildi.
                // - Pix.LoadFromFile: Resmi piksellerine ayırarak belleğe yükler.
                // - engine.Process(pix): Motor resmi tarar ve yazıları tanır.
                // - using blokları: İşlem bitince belleğin (RAM) derhal temizlenmesini sağlar.
                using var engine = new TesseractEngine(_tessDataPath, "tur", EngineMode.Default);
                using var pix = Pix.LoadFromFile(tempPath);
                using var page = engine.Process(pix);
                rawText = page.GetText().Trim(); // Okunan metnin başındaki ve sonundaki gereksiz boşlukları silip değişkene atar.
            }
            catch (Exception)
            {
                // İKİNCİ YOL (YEDEK PLAN / FALLBACK):
                // Bazı sistemlerde (örneğin Docker konteynerlarında veya Linux sunucularda) C# Tesseract DLL'leri sorun çıkarabilir.
                // Bu durumda programın çökmesini engelliyoruz ve işletim sisteminin kendi komut satırındaki (CLI)
                // "tesseract" programını harici bir alt işlem (Process) olarak başlatıp çalıştırıyoruz.
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "tesseract",                           // Çalıştırılacak terminal komutu
                    Arguments = $"\"{tempPath}\" stdout -l tur",       // Parametreler: resmi oku, ekrana (stdout) Türkçe bas
                    RedirectStandardOutput = true,                     // Terminal çıktısını C# içine oku
                    RedirectStandardError = true,                      // Varsa hata mesajlarını yakala
                    UseShellExecute = false,                           // Yeni bir siyah terminal penceresi açma
                    CreateNoWindow = true                              // Penceresiz gizli çalıştır
                };

                using var proc = System.Diagnostics.Process.Start(psi);
                if (proc != null)
                {
                    // Terminalin ekrana yazdığı metni sonuna kadar oku:
                    rawText = await proc.StandardOutput.ReadToEndAsync();
                    await proc.WaitForExitAsync(); // İşlemin bitmesini bekle
                    rawText = rawText.Trim();
                }
            }

            // 3. ADIM: METNİ AYRIŞTIRMA (PARSE ETME)
            // Okunan düz metni Parse fonksiyonuna göndererek içinden isim ve notu çekiyoruz.
            return Parse(rawText);
        }
        finally
        {
            // 4. ADIM: TEMİZLİK (CLEANUP)
            // Ne olursa olsun geçici oluşturduğumuz resim dosyasını diskten siliyoruz ki bilgisayarda çöp dosya birikmesin.
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    // ------------------------------------------------------------------------------------------
    // METOT: Parse (Öğrenci Adını ve Notunu Metinden Ayıklama)
    // Bu metot OCR'dan gelen karmaşık metin yığınını analiz eder.
    // ------------------------------------------------------------------------------------------
    private static OcrResult Parse(string rawText)
    {
        // Türkçe karakterlerde büyük/küçük harf ayrımından kaynaklı hataları önlemek için metni küçük harfe çeviriyoruz:
        var normalized = rawText.ToLowerInvariant();

        // --------------------------------------------------------------------------------------
        // 1. STRATEJİ: ETİKETLİ ARAMA (REGULAR EXPRESSIONS / REGEX)
        // Beklenen kağıt formatı:
        // "Öğrenci Adı : Ali İhsan"
        // "Öğrenci Notu : 80"
        //
        // Regex Deseni Açıklaması:
        // [oö]ğ?renc[iı] : OCR bazen 'ö' yerine 'o', 'i' yerine 'ı' okuyabilir, 'ğ'yi kaçırabilir. Bu desen hepsini kapsar!
        // \s+           : Bir veya daha fazla boşluk
        // ad[iı]        : "adi" veya "adı"
        // [:\-]         : İki nokta (:) veya tire (-) işareti
        // (.+)          : İki noktadan sonra gelen ismin tamamını yakala (1. Yakalama Grubu)
        // (\d{1,3})     : Not kısmında 1 ila 3 basamaklı bir sayı yakala (0 ile 100 arası notlar için)
        // --------------------------------------------------------------------------------------
        var nameMatch  = Regex.Match(normalized, @"[oö]ğ?renc[iı]\s+ad[iı]\s*[:\-]\s*(.+)", RegexOptions.IgnoreCase);
        var scoreMatch = Regex.Match(normalized, @"[oö]ğ?renc[iı]\s+notu\s*[:\-]\s*(\d{1,3})", RegexOptions.IgnoreCase);

        // Hem ad hem not deseni başarıyla bulunduysa ve yakalanan not geçerli bir tamsayıya (int) dönüştürülebiliyorsa:
        if (nameMatch.Success && scoreMatch.Success &&
            int.TryParse(scoreMatch.Groups[1].Value.Trim(), out int labeledScore))
        {
            // Öğrencinin adını küçük harfle değil, kağıttaki orijinal büyük/küçük harfleriyle (Ali İhsan şeklinde)
            // alabilmek için aynı regex'i ham metin (rawText) üzerinde tekrar çalıştırıyoruz:
            var rawNameMatch = Regex.Match(rawText, @"[oÖ]ğ?renc[iıİ]\s+ad[iıİ]\s*[:\-]\s*(.+)", RegexOptions.IgnoreCase);
            
            return new OcrResult
            {
                // Ham metinden isim yakalanabildiyse onu, yakalanamadıysa küçük harfli olanı kullan:
                StudentName = rawNameMatch.Success ? rawNameMatch.Groups[1].Value.Trim() : nameMatch.Groups[1].Value.Trim(),
                Score       = labeledScore,
                RawText     = rawText
            };
        }

        // --------------------------------------------------------------------------------------
        // 2. STRATEJİ: YEDEK FORMAT (FALLBACK - ETİKETSİZ TEK SATIR)
        // Kağıtta "Öğrenci Adı:" yazmıyorsa, sadece "Ali İhsan 80" yazıyorsa:
        // ^(.*?)\s+(\d{1,3})\s*$
        // Satırın başındaki kelimeleri isim, en sonundaki 1-3 basamaklı sayıyı not kabul et.
        // --------------------------------------------------------------------------------------
        var fallback = Regex.Match(rawText, @"^(.*?)\s+(\d{1,3})\s*$",
            RegexOptions.Multiline | RegexOptions.RightToLeft);

        if (fallback.Success && int.TryParse(fallback.Groups[2].Value, out int fallbackScore))
        {
            return new OcrResult
            {
                StudentName = fallback.Groups[1].Value.Trim(),
                Score       = fallbackScore,
                RawText     = rawText
            };
        }

        // --------------------------------------------------------------------------------------
        // 3. STRATEJİ: AYRIŞTIRMA TAMAMEN BAŞARISIZ OLURSA
        // Resim çok bulanıksa veya beklenmeyen bir yazı varsa programın çökmesini engelliyoruz.
        // Okunan tüm metni isim alanına koyup notu 0 yapıyoruz; böylece kullanıcı ekranda
        // bunu görüp manuel olarak klavyeden düzeltebilir.
        // --------------------------------------------------------------------------------------
        return new OcrResult { StudentName = rawText.Trim(), Score = 0, RawText = rawText };
    }
}
