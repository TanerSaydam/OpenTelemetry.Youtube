
using Microsoft.IO;
using System.Diagnostics;

namespace Observability.Youtube.WebAPI;

public sealed class ReqAndResActivityBodyMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        await AddReqBodyAsync(context);
        await AddResBodyAsync(context, next);
    }

    private static async Task AddResBodyAsync(HttpContext context, RequestDelegate next)
    {
        var orgResponse = context.Response.Body;

        RecyclableMemoryStreamManager recyclamemoryStreamManager = new();
        await using var responseBodyMemoryStream = recyclamemoryStreamManager.GetStream();
        context.Response.Body = responseBodyMemoryStream;

        await next(context);

        responseBodyMemoryStream.Position = 0;

        var responseBodyStreamReader = new StreamReader(responseBodyMemoryStream);
        var resBody = await responseBodyStreamReader.ReadToEndAsync();

        Activity.Current!.AddTag("http.res.body", resBody);
        context.Response.Body.Position = 0;

        await responseBodyMemoryStream.CopyToAsync(orgResponse);

    }

    private static async Task AddReqBodyAsync(HttpContext context)
    {
        context.Request.EnableBuffering();
        var requBodyStreamReader = new StreamReader(context.Request.Body);
        var reqBody = await requBodyStreamReader.ReadToEndAsync();
        Activity.Current!.AddTag("http.req.body", reqBody);
        context.Request.Body.Position = 0;
    }
}
