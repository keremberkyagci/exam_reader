using ExamReader.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExamReader.Api.Controllers;

[ApiController]
[Route("api/ocr")]
public class OcrController : ControllerBase
{
    private readonly IOcrService _ocrService;

    public OcrController(IOcrService ocrService)
    {
        _ocrService = ocrService;
    }

    // POST api/ocr
    // multipart/form-data: image=<file>
    [HttpPost]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB
    public async Task<IActionResult> Scan([FromForm] IFormFile image)
    {
        if (image is null || image.Length == 0)
            return BadRequest("Görüntü dosyası gereklidir.");

        var result = await _ocrService.ExtractAsync(image);
        return Ok(result);
    }
}
