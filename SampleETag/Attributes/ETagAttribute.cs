using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SampleETag.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false)]
public class ETagAttribute : ActionFilterAttribute
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var originalBody = context.HttpContext.Response.Body;
        using var buffer = new MemoryStream();
        context.HttpContext.Response.Body = buffer;
    
        var executedContext = await next();
        
        if (context.HttpContext.Response.StatusCode == StatusCodes.Status200OK)
        {
            buffer.Seek(0, SeekOrigin.Begin);
            var bodyBytes = new MemoryStream(buffer.ToArray()).ToArray();
            var etag = $"\"{Convert.ToBase64String(System.Security.Cryptography.MD5.HashData(bodyBytes))}\"";
        
            if (context.HttpContext.Request.Headers.TryGetValue("If-None-Match", out var inm) && inm == etag)
            {
                context.HttpContext.Response.Headers.ETag = etag;
                executedContext.Result = new StatusCodeResult(StatusCodes.Status304NotModified);
                return;
            }
        
            context.HttpContext.Response.Headers.ETag = etag;
        
            buffer.Seek(0, SeekOrigin.Begin);
            await buffer.CopyToAsync(originalBody);
            context.HttpContext.Response.Body = originalBody;
        }
    }
}