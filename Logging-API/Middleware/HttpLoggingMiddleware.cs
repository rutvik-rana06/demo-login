using System.Text;
using Microsoft.FeatureManagement;


public class HttpLoggingMiddleware
{
    private readonly ILogger _logger;
    private readonly RequestDelegate _next;
    private readonly IFeatureManager _featureManager;

    public HttpLoggingMiddleware(RequestDelegate next, ILogger<HttpLoggingMiddleware> logger, IFeatureManager featureManager)
    {
        _next = next;
        _logger = logger;
        _featureManager = featureManager;
    }

    public async Task Invoke(HttpContext context)
    {
        if (await _featureManager.IsEnabledAsync("IsLoggingEnabled"))
        {
            await LogRequest(context);

            var originalResponseBody = context.Response.Body;

            using (var responseBody = new MemoryStream())
            {
                context.Response.Body = responseBody;
                await _next.Invoke(context);

                await LogResponse(context, responseBody, originalResponseBody);
            }
        }
        else
        {
            await _next.Invoke(context);
        }

    }

    private async Task LogResponse(HttpContext context, MemoryStream responseBody, Stream originalResponseBody)
    {
        var responseContent = new StringBuilder();
        responseContent.AppendLine($"{DateTime.Now} === Response Info ===");

        responseContent.AppendLine($"{DateTime.Now} -- headers");
        foreach (var (headerKey, headerValue) in context.Response.Headers)
        {
            responseContent.AppendLine($"{DateTime.Now} header = {headerKey}    value = {headerValue}");
        }

        responseContent.AppendLine($"{DateTime.Now} -- body");
        responseBody.Position = 0;
        var content = await new StreamReader(responseBody).ReadToEndAsync();
        responseContent.AppendLine($"{DateTime.Now} body = {content}");
        responseBody.Position = 0;
        await responseBody.CopyToAsync(originalResponseBody);
        context.Response.Body = originalResponseBody;

        _logger.LogInformation(responseContent.ToString());
    }

    private async Task LogRequest(HttpContext context)
    {
        var requestContent = new StringBuilder();

        requestContent.AppendLine($"{DateTime.Now} === Request Info ===");
        requestContent.AppendLine($"{DateTime.Now} - method = {context.Request.Method.ToUpper()}");
        requestContent.AppendLine($"{DateTime.Now} path = {context.Request.Path}");

        requestContent.AppendLine($"{DateTime.Now} -- headers");
        foreach (var (headerKey, headerValue) in context.Request.Headers)
        {
            requestContent.AppendLine($"{DateTime.Now} header = {headerKey}    value = {headerValue}");
        }

        requestContent.AppendLine($"{DateTime.Now} -- body");
        context.Request.EnableBuffering();
        var requestReader = new StreamReader(context.Request.Body);
        var content = await requestReader.ReadToEndAsync();
        requestContent.AppendLine($"{DateTime.Now} body = {content}");

        _logger.LogInformation(requestContent.ToString());
        context.Request.Body.Position = 0;
    }





}