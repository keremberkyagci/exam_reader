// ==============================================================================================
// DOSYA ADI: ExamResultRepository.cs
// AÇIKLAMA: Bu dosya, veri tabanındaki "exam_results" (sınav sonuçları) tablosu ile ilgili
// okuma ve ekleme işlemlerini yürüten "Repository" (Veri Deposu) katmanıdır.
// Öğrencinin adı, aldığı not ve sınav kayıt tarihini veri tabanına yazar ve oradan okur.
// ==============================================================================================

// ----------------------------------------------------------------------------------------------
// "using" İFADELERİ
// ----------------------------------------------------------------------------------------------

// Dapper: Veri tabanı sorgularını C# nesnelerine hızlıca bağlayan kütüphane.
using Dapper;

// ExamResult: Bir sınav sonucunun yapısını (id, öğrenci adı, not vb.) temsil eden model sınıfı.
using ExamReader.Api.Models;

// IDbConnection: Veri tabanı bağlantısını temsil eden C# arayüzü.
using System.Data;

namespace ExamReader.Api.Repositories;

// ----------------------------------------------------------------------------------------------
// INTERFACE (ARAYÜZ) - IExamResultRepository
// Sınav sonuçları deposunun hangi işlemleri yapabileceğini belirten sözleşme.
// ----------------------------------------------------------------------------------------------
public interface IExamResultRepository
{
    // GetAllAsync: Kayıtlı tüm öğrenci sınav sonuçlarını bir liste olarak getirir.
    Task<IEnumerable<ExamResult>> GetAllAsync();

    // AddAsync: Yeni bir öğrenci sınav sonucunu veri tabanına kaydeder.
    // - Task: Geriye özel bir veri döndürmez (void gibidir), sadece işlemin tamamlandığını bildirir.
    // - ExamResult examResult: Kaydedilecek olan öğrenci adı ve notunu barındıran nesne.
    Task AddAsync(ExamResult examResult);
}

// ----------------------------------------------------------------------------------------------
// SOMUT SINIF (CLASS) - ExamResultRepository
// Yukarıdaki sözleşmeyi (IExamResultRepository) gerçek SQL komutlarıyla hayata geçiren sınıftır.
// ----------------------------------------------------------------------------------------------
public class ExamResultRepository : IExamResultRepository
{
    // _db: Veri tabanına SQL sorguları göndermemizi sağlayan açık bağlantı nesnesi.
    private readonly IDbConnection _db;

    // ------------------------------------------------------------------------------------------
    // CONSTRUCTOR (YAPICI METOT)
    // Sınıf oluşturulduğunda sistem tarafından veri tabanı bağlantısı buraya enjekte edilir.
    // ------------------------------------------------------------------------------------------
    public ExamResultRepository(IDbConnection db)
    {
        _db = db;
    }

    // ------------------------------------------------------------------------------------------
    // METOT: GetAllAsync
    // Veri tabanındaki tüm sınav sonuçlarını getirir.
    // ------------------------------------------------------------------------------------------
    public async Task<IEnumerable<ExamResult>> GetAllAsync()
    {
        // """ (Üç Tırnak - Raw String Literal): C#'ın modern özelliklerinden biridir.
        // Uzun ve çok satırlı SQL sorgularını araya '+' veya '\n' koymadan, tıpkı metin dosyasında
        // yazar gibi rahat ve okunabilir bir şekilde yazmamızı sağlar.
        const string sql = """
            SELECT
                id,
                student_name AS StudentName,
                score,
                created_at  AS CreatedAt
            FROM exam_results
            ORDER BY created_at DESC;
            """;

        // AS StudentName ve AS CreatedAt nedir?
        // Veri tabanlarında sütun isimleri genelde alt tireli (snake_case: student_name) yazılır.
        // C# sınıflarında ise kelimelerin baş harfi büyük yazılır (PascalCase: StudentName).
        // SQL'deki "AS" kelimesi, Dapper'ın veri tabanındaki "student_name" sütununu
        // C#'taki "StudentName" özelliğine kolayca eşleştirmesini (map etmesini) sağlar.

        // QueryAsync<ExamResult>: Sorguyu çalıştırır ve gelen satırları ExamResult listesi olarak döner.
        return await _db.QueryAsync<ExamResult>(sql);
    }

    // ------------------------------------------------------------------------------------------
    // METOT: AddAsync
    // Yeni okunan bir sınav sonucunu (öğrenci adı ve notu) veri tabanına yeni bir satır olarak ekler.
    // ------------------------------------------------------------------------------------------
    public async Task AddAsync(ExamResult examResult)
    {
        // INSERT INTO: Veri tabanına yeni veri eklemek için kullanılan standart SQL komutudur.
        // exam_results tablosunun 'student_name' ve 'score' sütunlarına yeni değerler ekleneceğini belirtir.
        // @StudentName ve @Score: Dapper'a parametre olarak aktarılacak değişken yer tutucularıdır.
        const string sql = """
            INSERT INTO exam_results
                (student_name, score)
            VALUES
                (@StudentName, @Score);
            """;

        // ExecuteAsync:
        // Veri tabanında bir değişiklik yapıldığında (INSERT, UPDATE, DELETE gibi) kullanılır.
        // SELECT gibi geriye satır listesi beklemediğimiz durumlar için idealdir.
        // Dapper, 'examResult' nesnesinin içindeki StudentName ve Score değerlerini otomatik olarak
        // @StudentName ve @Score yer tutucularının yerine güvenle yerleştirir.
        await _db.ExecuteAsync(sql, examResult);
    }
}
