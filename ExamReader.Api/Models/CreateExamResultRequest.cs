namespace ExamReader.Api.Models;

public class CreateExamResultRequest
{
    public string StudentName { get; set; } = string.Empty;
    public int Score { get; set; }
}
