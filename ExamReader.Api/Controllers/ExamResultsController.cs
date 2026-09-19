using ExamReader.Api.Models;
using ExamReader.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExamReader.Api.Controllers;

[ApiController]
[Route("api/exam-results")]
public class ExamResultsController : ControllerBase
{
    private readonly IExamResultService _service;

    public ExamResultsController(IExamResultService service)
    {
        _service = service;
    }

    // GET api/exam-results
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExamResult>>> GetAll()
    {
        var results = await _service.GetAllAsync();
        return Ok(results);
    }

    // POST api/exam-results
    // Body: { "studentName": "Ali İhsan", "score": 80 }
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateExamResultRequest request)
    {
        if (request.Score < 0 || request.Score > 100)
            return BadRequest("Score 0 ile 100 arasında olmalıdır.");

        var examResult = new ExamResult
        {
            StudentName = request.StudentName,
            Score       = request.Score
        };

        await _service.AddAsync(examResult);

        return Created();
    }
}
