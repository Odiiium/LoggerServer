using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace LoggerServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoggerController : ControllerBase
    {
        [HttpPost("log")]
        public async Task<IResult> PostLog([FromBody] LogDto log)
        {
            if (log == null || string.IsNullOrEmpty(log.UserId) || string.IsNullOrEmpty(log.Message))
                return Results.BadRequest("Log is null");

            try
            {
                var safeUserId = string.Join("_", log.UserId.Split(Path.GetInvalidFileNameChars()));
                var dir = Path.Combine("logs", safeUserId);

                Directory.CreateDirectory(dir);

                var filePath = Path.Combine(dir, $"{DateTime.UtcNow:yyyy-MM-dd}.log");

                var line = $"{DateTime.UtcNow:HH:mm:ss} | {log.Message}{Environment.NewLine}";

                await System.IO.File.AppendAllTextAsync(filePath, line, Encoding.UTF8);

                return Results.Ok();
            }
            catch
            {
                return Results.StatusCode(500);
            }
        }

        // GET /Logger/logs/{userId} - все даты для пользователя
        // GET /Logger/logs/{userId}/{date} - конкретный день (формат: yyyy-MM-dd)
        [HttpGet("logs/{userId}/{date?}")]
        public async Task<IResult> GetLogs(string userId, string? date = null)
        {
            var safeUserId = string.Join("_", userId.Split(Path.GetInvalidFileNameChars()));
            var dir = Path.Combine("logs", safeUserId);

            if (!Directory.Exists(dir))
                return Results.NotFound("No logs for this user");

            if (date == null)
            {
                var files = Directory.GetFiles(dir, "*.log")
                    .Select(f => Path.GetFileNameWithoutExtension(f))
                    .OrderDescending()
                    .ToList();

                return Results.Ok(files);
            }

            var safeDate = string.Join("_", date.Split(Path.GetInvalidFileNameChars()));
            var filePath = Path.Combine(dir, $"{safeDate}.log");

            if (!System.IO.File.Exists(filePath))
                return Results.NotFound($"No logs for {date}");

            var content = await System.IO.File.ReadAllTextAsync(filePath, Encoding.UTF8);
            return Results.Text(content, "text/plain");
        }
    }
}
