// ==============================================================================================
// DOSYA ADI: ExamRepository.cs
// AÇIKLAMA: Bu dosya "Repository" (Depo) tasarım kalıbını uygular.
// Görevi: "exams" (sınavlar) veri tabanı tablosuyla doğrudan iletişim kurmak, SQL sorgularını
// çalıştırmak ve gelen verileri C# nesnelerine dönüştürmektir.
// ==============================================================================================

// ----------------------------------------------------------------------------------------------
// "using" İFADELERİ
// ----------------------------------------------------------------------------------------------

// Dapper: Veri tabanından gelen satırları otomatik olarak C# nesnelerine eşleyen (map eden)
// çok hızlı ve popüler bir kütüphanedir (Micro-ORM).
using Dapper;

// Exam model sınıfını (veri yapısını) projenin Models klasöründen içeri aktarır.
using ExamReader.Api.Models;

// IDbConnection gibi veri tabanı bağlantı standartlarını kullanabilmek için gereklidir.
using System.Data;

// namespace: Kodlarımızı düzenli tutmak için kullandığımız mantıksal klasörleme sistemidir.
namespace ExamReader.Api.Repositories;

// ----------------------------------------------------------------------------------------------
// INTERFACE (ARAYÜZ) - IExamRepository
// Interface, bir sınıfın "ne yapacağını" söyleyen ama "nasıl yapacağını" belirtmeyen bir sözleşmedir.
// Buradaki amaç, sistemin diğer kısımlarının veri tabanına bağımlı olmadan sadece bu sözleşmeye
// bakarak çalışabilmesini sağlamaktır (Gevşek Bağlılık - Loose Coupling).
// ----------------------------------------------------------------------------------------------
public interface IExamRepository
{
    // GetAllAsync: Tüm sınavları getirmeyi taahhüt eden asenkron bir metot tanımı.
    // - Task: Bu işlemin zaman alacağını ve arka planda asenkron (programı dondurmadan) çalışacağını belirtir.
    // - IEnumerable<Exam>: Birden fazla Exam (Sınav) nesnesi içeren bir liste/dizi döndüreceğini belirtir.
    Task<IEnumerable<Exam>> GetAllAsync();

    // GetByIdAsync: Verilen 'id' numarasına sahip tek bir sınavı getirmeyi taahhüt eder.
    // - Exam?: Soru işareti (?), aranan sınav veri tabanında bulunamazsa sonucun 'null' (boş) dönebileceğini ifade eder.
    Task<Exam?> GetByIdAsync(int id);
}

// ----------------------------------------------------------------------------------------------
// SOMUT SINIF (CLASS) - ExamRepository
// Yukarıdaki 'IExamRepository' sözleşmesini kabul eden ve içindeki metotların gerçekten
// SQL sorgularıyla nasıl çalışacağını yazan sınıftır.
// ----------------------------------------------------------------------------------------------
public class ExamRepository : IExamRepository
{
    // _db: Veri tabanı ile konuşmamızı sağlayan bağlantı köprüsüdür.
    // - private: Bu değişkene sadece bu sınıfın içinden erişilebilir, dışarıya kapalıdır.
    // - readonly: Değeri sadece kurucu metotta (constructor) atanabilir, daha sonra kazara değiştirilemez.
    private readonly IDbConnection _db;

    // ------------------------------------------------------------------------------------------
    // CONSTRUCTOR (YAPICI METOT)
    // Bu sınıftan yeni bir nesne üretildiğinde İLK çalışan özel metottur.
    // Dependency Injection (Bağımlılık Enjeksiyonu) sayesinde Program.cs'te tanımladığımız
    // PostgreSQL bağlantısı buraya otomatik olarak parametre (db) olarak gönderilir.
    // ------------------------------------------------------------------------------------------
    public ExamRepository(IDbConnection db)
    {
        _db = db; // Gelen veri tabanı bağlantısını sınıfın içindeki _db değişkenine kaydediyoruz.
    }

    // ------------------------------------------------------------------------------------------
    // METOT: GetAllAsync
    // Veri tabanındaki tüm sınav kayıtlarını en yeniden en eskiye doğru sıralayarak getirir.
    // - async: Metodun içinde 'await' (bekleme) yapılacağını belirtir.
    // ------------------------------------------------------------------------------------------
    public async Task<IEnumerable<Exam>> GetAllAsync()
    {
        // Çalıştırılacak SQL sorgusu:
        // exams tablosundaki id, title (başlık) ve created_at (oluşturulma tarihi) sütunlarını seç.
        // ORDER BY created_at DESC: En son oluşturulan en başta olacak şekilde tersten sırala.
        const string sql = "SELECT id, title, created_at FROM exams ORDER BY created_at DESC";

        // _db.QueryAsync<Exam>(sql):
        // Dapper kütüphanesi bu SQL'i veri tabanında çalıştırır.
        // Gelen sonuçlardaki sütunları otomatik olarak 'Exam' modelinin özellikleriyle eşleştirip bir liste olarak döndürür.
        // 'await': Veri tabanından cevap gelene kadar sistemi kilitlemeden sabırla bekler.
        return await _db.QueryAsync<Exam>(sql);
    }

    // ------------------------------------------------------------------------------------------
    // METOT: GetByIdAsync
    // Belirli bir kimlik numarasına (id) sahip tek bir sınavı veri tabanından sorgular.
    // ------------------------------------------------------------------------------------------
    public async Task<Exam?> GetByIdAsync(int id)
    {
        // SQL sorgusu:
        // WHERE id = @Id: Belirtilen id değerine sahip satırı filtreler.
        // DİKKAT: Doğrudan "WHERE id = " + id yazmak yerine '@Id' şeklinde parametre kullanıyoruz.
        // Bu yöntem "SQL Injection" denilen tehlikeli veri tabanı saldırılarına karşı kesin güvenlik sağlar!
        const string sql = "SELECT id, title, created_at FROM exams WHERE id = @Id";

        // QueryFirstOrDefaultAsync:
        // Sorguyu çalıştırır, eşleşen ilk kaydı getirir. Eğer eşleşen kayıt yoksa 'null' döndürür.
        // 'new { Id = id }': @Id parametresinin yerine fonksiyona gelen 'id' değerini güvenle yerleştirir.
        return await _db.QueryFirstOrDefaultAsync<Exam>(sql, new { Id = id });
    }
}
