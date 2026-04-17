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
    }
}
