using Dapper;
using ExamReader.Api.Models;
using System.Data;

namespace ExamReader.Api.Repositories;

public interface IExamRepository
{
    Task<IEnumerable<Exam>> GetAllAsync();
    Task<Exam?> GetByIdAsync(int id);
}

public class ExamRepository : IExamRepository
{
    private readonly IDbConnection _db;

    public ExamRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Exam>> GetAllAsync()
    {
        const string sql = "SELECT id, title, created_at FROM exams ORDER BY created_at DESC";
        return await _db.QueryAsync<Exam>(sql);
    }

    public async Task<Exam?> GetByIdAsync(int id)
    {
        const string sql = "SELECT id, title, created_at FROM exams WHERE id = @Id";
        return await _db.QueryFirstOrDefaultAsync<Exam>(sql, new { Id = id });
    }
}
