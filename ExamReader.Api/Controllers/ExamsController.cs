using ExamReader.Api.Models;
using ExamReader.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExamReader.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExamsController : ControllerBase
{
    private readonly IExamService _service;

    public ExamsController(IExamService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Exam>>> GetAll()
    {
        var exams = await _service.GetAllExamsAsync();
        return Ok(exams);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Exam>> GetById(int id)
    {
        var exam = await _service.GetExamByIdAsync(id);
        if (exam is null)
            return NotFound();

        return Ok(exam);
    }
}
