// =============================================================================
// AD ALANI (NAMESPACE) TANIMI
// =============================================================================
// "namespace", ilgili sınıfları bir arada tutan mantıksal bir klasördür.
// Bu model dosyasının "ExamReader.Api.Models" altında yer aldığını belirtir.
namespace ExamReader.Api.Models;

// =============================================================================
// SINIF (CLASS) TANIMI - SINAV SONUCU MODELİ (ENTITY)
// =============================================================================
// "public": Sınıfın projedeki her yerden erişilebilir olmasını sağlar.
// "class": Nesnelerimizi üretmek için kullandığımız şablon / kalıptır.
// "ExamResult": Bu sınıf, bir öğrencinin girmiş olduğu sınavın sonucunu temsil eder.
// Veritabanında bir sınav sonucuna dair saklanan tüm bilgileri (kimlik, öğrenci adı,
// puan ve kayıt tarihi) tek bir çatı altında toplar.
public class ExamResult
{
    // =========================================================================
    // BENZERSİZ KİMLİK NUMARASI (PRIMARY KEY / ID)
    // =========================================================================
    // "int": Tam sayı türü (1, 2, 3 vb.).
    // "Id": Bu sınav sonucuna özel, sistem tarafından verilen tekil kimlik numarasıdır.
    // Her bir sonucun farklı bir Id'si olur; böylece aynı isimde öğrenciler olsa bile karışıklık yaşanmaz.
    // "{ get; set; }": Değerin okunmasını (get) ve değiştirilmesini (set) mümkün kılar.
    public int Id { get; set; }

    // =========================================================================
    // ÖĞRENCİ ADI VE SOYADI
    // =========================================================================
    // "string": Yazı veya metin verilerini tutmak için kullanılan tiptir.
    // "StudentName": Sınava katılan öğrencinin adını tutar (Örn: "Ayşe Kaya").
    // "{ get; set; }": İsmin okunabilmesini ve yeni bir isim atanabilmesini sağlar.
    // "= string.Empty;": Değişkenin boşta kalmasını (null olmasını) engelleyerek başlangıçta boş metin atar.
    public string StudentName { get; set; } = string.Empty;

    // =========================================================================
    // SINAV PUANI
    // =========================================================================
    // "int": Tam sayı türüdür.
    // "Score": Öğrencinin sınav kağıdından aldığı başarı puanıdır (Örn: 85, 100).
    // "{ get; set; }": Not bilgisinin okunmasını ve güncellenmesini sağlar.
    public int Score { get; set; }

    // =========================================================================
    // KAYIT TARİHİ VE SAATİ
    // =========================================================================
    // "DateTime": Zaman bilgilerini (gün, ay, yıl, saat, dakika, saniye) saklayan tiptir.
    // "CreatedAt": Bu sınav sonucunun sisteme veya veritabanına tam olarak ne zaman eklendiğini belirtir.
    // "{ get; set; }": Tarih bilgisini okuma ve yazma izni verir.
    public DateTime CreatedAt { get; set; }
}
