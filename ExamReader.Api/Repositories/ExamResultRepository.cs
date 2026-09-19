using Dapper;
using ExamReader.Api.Models;
using System.Data;

namespace ExamReader.Api.Repositories;

public interface IExamResultRepository
{
    Task<IEnumerable<ExamResult>> GetAllAsync();
    Task AddAsync(ExamResult examResult);
}

public class ExamResultRepository : IExamResultRepository
{
    private readonly IDbConnection _db;

    public ExamResultRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<IEnumerable<ExamResult>> GetAllAsync()
    {
        const string sql = """
            SELECT
                id,
                student_name AS StudentName,
                score,
                created_at  AS CreatedAt
            FROM exam_results
            ORDER BY created_at DESC;
            """;

        return await _db.QueryAsync<ExamResult>(sql);
    }

    public async Task AddAsync(ExamResult examResult)
    {
        const string sql = """
            INSERT INTO exam_results
                (student_name, score)
            VALUES
                (@StudentName, @Score);
            """;

        await _db.ExecuteAsync(sql, examResult);
    }
}
