
namespace SampleETag.CustomMiddlewares;

public class ETagMiddleware(RequestDelegate next, ILogger<ETagMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var originalBody = context.Response.Body;
        var buffer = new MemoryStream();
        context.Response.Body = buffer;

        await next(context);
        if (context.Response.StatusCode == StatusCodes.Status200OK && buffer.Length > 0)
        {
            buffer.Seek(0, SeekOrigin.Begin);
            var bodyBytes = buffer.ToArray();
            var etag = $"\"{Convert.ToBase64String(System.Security.Cryptography.MD5.HashData(bodyBytes))}\"";

            context.Response.Headers.ETag = etag;
            
            if (context.Request.Headers.TryGetValue("If-None-Match", out var inm) && inm == etag)
            {
                Console.WriteLine("ETag matched. Returning 304 Not Modified.");

                context.Response.Headers.ContentLength = 0;

                context.Response.StatusCode = StatusCodes.Status304NotModified;

                return;
            }
            buffer.Seek(0, SeekOrigin.Begin);
            await buffer.CopyToAsync(originalBody);
        }
        else
        {
            context.Response.Body = originalBody;
        }
    }
}