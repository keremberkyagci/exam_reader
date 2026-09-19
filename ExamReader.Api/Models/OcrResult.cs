namespace ExamReader.Api.Models;

public class OcrResult
{
    public string StudentName { get; set; } = string.Empty;
    public int Score { get; set; }
    public string RawText { get; set; } = string.Empty;  // debug
}
