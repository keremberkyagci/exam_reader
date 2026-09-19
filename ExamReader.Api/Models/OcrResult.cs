// =============================================================================
// AD ALANI (NAMESPACE) TANIMI
// =============================================================================
// "namespace", bu dosyanın ait olduğu mantıksal çalışma alanını ("ExamReader.Api.Models") ifade eder.
namespace ExamReader.Api.Models;

// =============================================================================
// SINIF (CLASS) TANIMI - OCR TARAMA SONUCU MODELİ
// =============================================================================
// "public": Sınıfa her yerden erişilebileceğini belirtir.
// "class": Bir nesnenin özelliklerini tanımlayan şablondur.
//
// "OCR" NEDİR?
// "Optical Character Recognition" (Optik Karakter Tanıma) ifadesinin kısaltmasıdır.
// Bilgisayarlar resimlerdeki yazıları doğrudan anlayamaz, sadece pikselleri (renkli noktaları) görür.
// OCR teknolojisi ise bir sınav kağıdı fotoğrafındaki harf ve rakamları tanıyıp
// bunları bilgisayarın okuyabileceği dijital metinlere dönüştürür.
//
// "OcrResult" Sınıfı:
// Sınav kağıdının fotoğrafı taranıp okunduktan sonra elde edilen tüm sonuçları
// (öğrenci adı, puan ve kağıttaki tüm ham yazılar) tek bir paket olarak tutmak için kullanılır.
public class OcrResult
{
    // =========================================================================
    // OKUNAN ÖĞRENCİ ADI
    // =========================================================================
    // "string": Metin türünde veri saklar.
    // "StudentName": OCR motorunun sınav kağıdının isim kısmından okuyup çıkardığı öğrenci adıdır.
    // "{ get; set; }": Bu ismin okunabilmesini ve atanabilmesini sağlar.
    // "= string.Empty;": Başlangıçta boş metin atanarak tanımsız (null) kalması önlenir.
    public string StudentName { get; set; } = string.Empty;

    // =========================================================================
    // OKUNAN VEYA HESAPLANAN SINAV PUANI
    // =========================================================================
    // "int": Tam sayı türüdür (0, 75, 100 vb.).
    // "Score": OCR sisteminin sınav kağıdından tespit ettiği ya da doğru/yanlışlara göre hesapladığı nottur.
    // "{ get; set; }": Puanın okunabilmesini ve değiştirilebilmesini sağlar.
    public int Score { get; set; }

    // =========================================================================
    // HAM METİN (RAW TEXT) - HATA AYIKLAMA (DEBUG) İÇİN
    // =========================================================================
    // "RawText": Kağıdın üzerindeki tüm yazıların, hiçbir filtreleme veya ayıklama yapılmadan,
    // olduğu gibi tek bir metin halinde tutulduğu alandır.
    // Neden Saklanır? ("debug"):
    // Eğer sistem öğrenci adını veya puanı yanlış okursa, kağıttan gerçekte ne okunduğunu
    // buraya bakarak görebiliriz. Böylece yazılımdaki hatayı (bug) bulup düzeltmek çok kolaylaşır.
    public string RawText { get; set; } = string.Empty;  // debug
}
