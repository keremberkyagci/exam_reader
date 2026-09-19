using ExamReader.Api.Models;
using Tesseract;
using System.Text.RegularExpressions;

namespace ExamReader.Api.Services;

public interface IOcrService
{
    Task<OcrResult> ExtractAsync(IFormFile image);
}

public class OcrService : IOcrService
{
    private readonly string _tessDataPath;

    public OcrService(IWebHostEnvironment env)
    {
        _tessDataPath = Path.Combine(env.ContentRootPath, "tessdata");
    }

    public async Task<OcrResult> ExtractAsync(IFormFile image)
    {
        // Görüntüyü geçici dosyaya yaz
        var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}");
        await using (var stream = File.Create(tempPath))
            await image.CopyToAsync(stream);

        try
        {
            string rawText = string.Empty;
            try
            {
                using var engine = new TesseractEngine(_tessDataPath, "tur", EngineMode.Default);
                using var pix = Pix.LoadFromFile(tempPath);
                using var page = engine.Process(pix);
                rawText = page.GetText().Trim();
            }
            catch (Exception)
            {
                // Linux / Container CLI Fallback: tesseract CLI aracılığıyla doğrudan oku
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "tesseract",
                    Arguments = $"\"{tempPath}\" stdout -l tur",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var proc = System.Diagnostics.Process.Start(psi);
                if (proc != null)
                {
                    rawText = await proc.StandardOutput.ReadToEndAsync();
                    await proc.WaitForExitAsync();
                    rawText = rawText.Trim();
                }
            }

            return Parse(rawText);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    // "öğrenci adı : Ali İhsan" + "öğrenci notu : 80" formatı
    private static OcrResult Parse(string rawText)
    {
        var normalized = rawText.ToLowerInvariant();

        // Regex ile daha esnek arama (ö/o, ğ/g, ı/i varyasyonlarını kapsar)
        var nameMatch  = Regex.Match(normalized, @"[oö]ğ?renc[iı]\s+ad[iı]\s*[:\-]\s*(.+)", RegexOptions.IgnoreCase);
        var scoreMatch = Regex.Match(normalized, @"[oö]ğ?renc[iı]\s+notu\s*[:\-]\s*(\d{1,3})", RegexOptions.IgnoreCase);

        if (nameMatch.Success && scoreMatch.Success &&
            int.TryParse(scoreMatch.Groups[1].Value.Trim(), out int labeledScore))
        {
            // Ham metinden büyük/küçük harf formatını korumak için aynı regex'i ham metin üzerinde çalıştır
            var rawNameMatch = Regex.Match(rawText, @"[oÖ]ğ?renc[iıİ]\s+ad[iıİ]\s*[:\-]\s*(.+)", RegexOptions.IgnoreCase);
            return new OcrResult
            {
                StudentName = rawNameMatch.Success ? rawNameMatch.Groups[1].Value.Trim() : nameMatch.Groups[1].Value.Trim(),
                Score       = labeledScore,
                RawText     = rawText
            };
        }

        // Fallback: tek satır "Ali İhsan 80"
        var fallback = Regex.Match(rawText, @"^(.*?)\s+(\d{1,3})\s*$",
            RegexOptions.Multiline | RegexOptions.RightToLeft);

        if (fallback.Success && int.TryParse(fallback.Groups[2].Value, out int fallbackScore))
        {
            return new OcrResult
            {
                StudentName = fallback.Groups[1].Value.Trim(),
                Score       = fallbackScore,
                RawText     = rawText
            };
        }

        // Parse tamamen başarısız — kullanıcı düzeltir
        return new OcrResult { StudentName = rawText.Trim(), Score = 0, RawText = rawText };
    }
}
