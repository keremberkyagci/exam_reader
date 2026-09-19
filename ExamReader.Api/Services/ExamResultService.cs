// ==============================================================================================
// DOSYA ADI: ExamResultService.cs
// AÇIKLAMA: Bu dosya "İş Mantığı Katmanı"nı (Business Logic Layer / Service Layer) temsil eder.
// Controller (dış dünya) ile Repository (veri tabanı) arasında bir köprü vazifesi görür.
// Sınav sonuçları ile ilgili yapılacak her türlü iş kuralı, kontrol ve yönlendirme burada yapılır.
// ==============================================================================================

// ----------------------------------------------------------------------------------------------
// "using" İFADELERİ
// ----------------------------------------------------------------------------------------------

// ExamResult modelini (öğrenci sınav sonucu veri kalıbını) kullanmak için içeri aktarır.
using ExamReader.Api.Models;

// Veri tabanına erişecek olan IExamResultRepository arayüzünü içeri aktarır.
using ExamReader.Api.Repositories;

namespace ExamReader.Api.Services;

// ----------------------------------------------------------------------------------------------
// INTERFACE (ARAYÜZ) - IExamResultService
// Bu servisin hangi işleri sunabileceğini belirten sözleşmedir.
// Controller sınıfları bu servisi kullanırken doğrudan sınıfa değil, bu arayüze bağlanır.
// ----------------------------------------------------------------------------------------------
public interface IExamResultService
{
    // Tüm sınav sonuçlarını getiren fonksiyon tanımı
    Task<IEnumerable<ExamResult>> GetAllAsync();

    // Yeni bir sınav sonucu ekleyen fonksiyon tanımı
    Task AddAsync(ExamResult examResult);
}

// ----------------------------------------------------------------------------------------------
// SOMUT SINIF (CLASS) - ExamResultService
// IExamResultService sözleşmesini uygulayan gerçek sınıftır.
// ----------------------------------------------------------------------------------------------
public class ExamResultService : IExamResultService
{
    // Servisin veri tabanıyla konuşabilmesi için bir "Repository"ye ihtiyacı vardır.
    // DİKKAT: Burada doğrudan somut sınıf olan 'ExamResultRepository' yerine,
    // onun arayüzü olan 'IExamResultRepository' kullanılır.
    // Buna "Dependency Inversion" (Bağımlılıkların Tersine Çevrilmesi) denir; kodun esnek olmasını sağlar.
    private readonly IExamResultRepository _repository;

    // ------------------------------------------------------------------------------------------
    // CONSTRUCTOR (YAPICI METOT)
    // Dependency Injection (Bağımlılık Enjeksiyonu):
    // Sistem, bu servis çalışırken ihtiyaç duyduğu depo nesnesini (repository) buraya otomatik getirir.
    // ------------------------------------------------------------------------------------------
    public ExamResultService(IExamResultRepository repository)
    {
        _repository = repository;
    }

    // ------------------------------------------------------------------------------------------
    // METOT: GetAllAsync
    // Tüm sınav sonuçlarını depodan (Repository) ister ve yukarıya (Controller'a) iletir.
    //
    // NOT (=> Ok Sözdizimi - Expression-bodied Member):
    // C#'ta bir fonksiyon tek bir satırdan ibaretse, süslü parantezler '{ return ...; }' yerine
    // '=>' (ok) simgesi kullanılarak daha kısa ve temiz bir şekilde yazılabilir.
    // ------------------------------------------------------------------------------------------
    public Task<IEnumerable<ExamResult>> GetAllAsync() =>
        _repository.GetAllAsync();

    // ------------------------------------------------------------------------------------------
    // METOT: AddAsync
    // Yeni gelen bir sınav sonucunu kaydetmesi için depoya iletir.
    // (İleride örneğin "not 0 ile 100 arasında mı?" gibi ek güvenlik kontrolleri veya
    // öğrenciye e-posta atma gibi işler yapılmak istenirse tam olarak buraya eklenecektir).
    // ------------------------------------------------------------------------------------------
    public Task AddAsync(ExamResult examResult) =>
        _repository.AddAsync(examResult);
}
