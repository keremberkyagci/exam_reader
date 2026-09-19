// =============================================================================
// AD ALANI (NAMESPACE) TANIMI
// =============================================================================
// "namespace" (ad alanı), sınıflarımızı mantıksal bir çatı altında toplayan isim alanıdır.
// Bu dosyanın "ExamReader.Api.Models" isimli ad alanında bulunduğunu belirtir.
namespace ExamReader.Api.Models;

// =============================================================================
// SINIF (CLASS) TANIMI - ANA SINAV MODELİ (ENTITY)
// =============================================================================
// "public": Bu sınıfa projedeki tüm dosyalardan serbestçe ulaşılabileceğini ifade eder.
// "class": Bir nesnenin özelliklerini tanımladığımız şablondur.
// "Exam": Bu sınıf, sistemdeki bir "Sınav"ı (örneğin "Fizik 1. Vize Sınavı") temsil eder.
// Veritabanında bir sınava ait hangi bilgilerin saklanacağını belirleyen temel yapıdır.
public class Exam
{
    // =========================================================================
    // BENZERSİZ KİMLİK NUMARASI (PRIMARY KEY / ID)
    // =========================================================================
    // "int": Tam sayı veri türüdür (1, 2, 3, 100 vb.).
    // "Id": "Identity" (Kimlik) kelimesinin kısaltmasıdır.
    // Her sınavın kendine ait benzersiz bir numarası vardır (Tıpkı TC Kimlik Numarası gibi).
    // İki sınavın adı aynı olsa bile "Id" numaraları farklı olacağı için birbirine karışmazlar.
    // "{ get; set; }": Değerin okunabilmesini (get) ve değiştirilebilmesini (set) sağlar.
    public int Id { get; set; }

    // =========================================================================
    // SINAV BAŞLIĞI / ADI
    // =========================================================================
    // "string": Yazı ve metin ifadelerini saklamak için kullanılan veri tipidir.
    // "Title": Sınavın başlığını veya adını tutar (Örn: "Matematik 101 - Vize").
    // "{ get; set; }": Başlık bilgisinin okunup yazılabilmesini sağlar.
    // "= string.Empty;": Başlangıçta boş metin ("") atayarak değerin "null" (belirsiz) kalmasını önler.
    public string Title { get; set; } = string.Empty;

    // =========================================================================
    // OLUŞTURULMA TARİHİ VE SAATİ
    // =========================================================================
    // "DateTime": C# dilinde tarih ve saat bilgisini (yıl, ay, gün, saat, dakika, saniye)
    // saklamak için kullanılan özel bir veri tipidir.
    // "CreatedAt": Bu sınav kaydının sisteme hangi tarih ve saatte eklendiğini tutar.
    // "{ get; set; }": Tarih bilgisinin okunabilmesini ve güncellenebilmesini sağlar.
    public DateTime CreatedAt { get; set; }
}
