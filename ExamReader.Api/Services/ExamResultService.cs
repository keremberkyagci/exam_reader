using ExamReader.Api.Models;
using ExamReader.Api.Repositories;

namespace ExamReader.Api.Services;

public interface IExamResultService
{
    Task<IEnumerable<ExamResult>> GetAllAsync();
    Task AddAsync(ExamResult examResult);
}

public class ExamResultService : IExamResultService
{
    private readonly IExamResultRepository _repository;

    public ExamResultService(IExamResultRepository repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<ExamResult>> GetAllAsync() =>
        _repository.GetAllAsync();

    public Task AddAsync(ExamResult examResult) =>
        _repository.AddAsync(examResult);
}
