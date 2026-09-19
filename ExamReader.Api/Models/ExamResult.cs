namespace ExamReader.Api.Models;

public class ExamResult
{
    public int Id { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public int Score { get; set; }

    public DateTime CreatedAt { get; set; }
}
