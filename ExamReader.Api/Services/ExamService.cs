using ExamReader.Api.Models;
using ExamReader.Api.Repositories;
using StackExchange.Redis;
using System.Text.Json;

namespace ExamReader.Api.Services;

public interface IExamService
{
    Task<IEnumerable<Exam>> GetAllExamsAsync();
    Task<Exam?> GetExamByIdAsync(int id);
}

public class ExamService : IExamService
{
    private readonly IExamRepository _repository;
    private readonly IDatabase _redis;

    public ExamService(IExamRepository repository, IConnectionMultiplexer redis)
    {
        _repository = repository;
        _redis = redis.GetDatabase();
    }

    public async Task<IEnumerable<Exam>> GetAllExamsAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Exam?> GetExamByIdAsync(int id)
    {
        string cacheKey = $"exam:{id}";

        var cached = await _redis.StringGetAsync(cacheKey);
        if (cached.HasValue)
            return JsonSerializer.Deserialize<Exam>((string)cached!);

        var exam = await _repository.GetByIdAsync(id);
        if (exam is not null)
            await _redis.StringSetAsync(cacheKey, JsonSerializer.Serialize(exam), TimeSpan.FromMinutes(5));

        return exam;
    }
}
