using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Api;

public sealed partial class CorsFunction
{
    private readonly ILogger<CorsFunction> _logger;
    private static readonly List<string> AllowedOrigins = new() { "http://localhost:4200", "https://salmon-pond-0870a1e00.2.azurestaticapps.net" };
    private static readonly Regex PreviewEnvironment =
        new("https:\\/\\/salmon-pond-0870a1e00-\\d+\\.eastasia\\.2\\.azurestaticapps\\.net", RegexOptions.None, TimeSpan.FromMilliseconds(100));

    public CorsFunction(ILogger<CorsFunction> logger)
    {
        _logger = logger;
    }

    [Function("NoCorsOptions")]
    public IActionResult RunNoCorsOptions(
        [HttpTrigger(AuthorizationLevel.Anonymous, "options", Route = "no-cors")]
        HttpRequest request)
    {
        LogCors(request.Path, request.Headers);

        LogNoCorsEndpoint();

        return new OkResult();
    }

    [Function("NoCorsGet")]
    public static IActionResult RunNoCorsGet(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "no-cors")]
        HttpRequest request)
    {
        return new OkResult();
    }

    [Function("CorsNoCacheOptions")]
    public IActionResult RunCorsNoCacheOptions(
        [HttpTrigger(AuthorizationLevel.Anonymous, "options", Route = "cors-no-cache")]
        HttpRequest request)
    {
        LogCors(request.Path, request.Headers);

        if (!TryGetCorsHeaders(request.Headers, out var origin))
        {
            return new OkResult();
        }

        SetCorsResponseHeaders(request, origin);
        return new OkResult();
    }

    [Function("CorsNoCacheGet")]
    public IActionResult RunCorsNoCacheGet(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "cors-no-cache")]
        HttpRequest request)
    {
        if (!TryGetAuthorizationHeader(request.Headers, out var authorization))
        {
            return new UnauthorizedResult();
        }

        if (!TryGetOriginHeader(request.Headers, out var origin))
        {
            return new BadRequestResult();
        }

        request.HttpContext.Response.Headers.Append("Access-Control-Allow-Origin", origin);
        return new OkObjectResult(new { Authorization = authorization });
    }

    [Function("CorsCacheOptions")]
    public IActionResult RunCorsCacheOptions(
        [HttpTrigger(AuthorizationLevel.Anonymous, "options", Route = "cors-cache/{id:int?}")]
        HttpRequest request)
    {
        LogCors(request.Path, request.Headers);

        if (!TryGetCorsHeaders(request.Headers, out var origin))
        {
            return new OkResult();
        }

        SetCorsResponseHeaders(request, origin);
        request.HttpContext.Response.Headers.Append("Access-Control-Max-Age", "600");
        return new OkResult();
    }

    [Function("CorsCacheGet")]
    public IActionResult RunCorsCacheGet(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "cors-cache/{id:int?}")]
        HttpRequest request)
    {
        if (!TryGetAuthorizationHeader(request.Headers, out var authorization))
        {
            return new UnauthorizedResult();
        }

        if (!TryGetOriginHeader(request.Headers, out var origin))
        {
            return new BadRequestResult();
        }

        request.HttpContext.Response.Headers.Append("Access-Control-Allow-Origin", origin);
        return new OkObjectResult(new { Authorization = authorization });
    }

    private void LogCors(string path, IHeaderDictionary headers)
    {
        if (!_logger.IsEnabled(LogLevel.Information))
        {
            return;
        }

        LogCorsRequest(
            path,
            headers["origin"],
            headers["access-control-request-headers"],
            headers["access-control-request-method"]);
    }

    private bool TryGetCorsHeaders(IHeaderDictionary headers, out string origin)
    {
        return TryGetOriginHeader(headers, out origin)
               && ValidateCorsMethodHeader(headers)
               && ValidateCorsHeadersHeader(headers);
    }

    private bool TryGetOriginHeader(IHeaderDictionary headers, out string origin)
    {
        origin = null;

        if (!headers.TryGetValue("Origin", out var originHeader))
        {
            LogOriginMissing();
            return false;
        }

        if (originHeader.Count != 1)
        {
            LogOriginNotSingle(originHeader);
            return false;
        }

        var tentativeOrigin = originHeader[0];

        if (!AllowedOrigins.Contains(tentativeOrigin) && !PreviewEnvironment.IsMatch(tentativeOrigin))
        {
            LogOriginNotAllowed(tentativeOrigin, AllowedOrigins);
            return false;
        }

        origin = tentativeOrigin;
        return true;
    }

    private bool ValidateCorsMethodHeader(IHeaderDictionary headers)
    {
        if (!headers.TryGetValue("Access-Control-Request-Method", out var methodHeader))
        {
            LogCorsMethodMissing();
            return false;
        }

        if (methodHeader.Count != 1)
        {
            LogCorsMethodNotSingle(methodHeader);
            return false;
        }

        if (!"GET".Equals(methodHeader[0], StringComparison.OrdinalIgnoreCase))
        {
            LogCorsMethodNotGet(methodHeader[0]);
            return false;
        }

        return true;
    }

    private bool ValidateCorsHeadersHeader(IHeaderDictionary headers)
    {
        if (!headers.TryGetValue("Access-Control-Request-Headers", out var headersHeader))
        {
            LogCorsHeadersMissing();
            return false;
        }

        if (headersHeader.Count != 1)
        {
            LogCorsHeadersNotSingle(headersHeader);
            return false;
        }

        if (!"authorization".Equals(headersHeader[0], StringComparison.OrdinalIgnoreCase))
        {
            LogCorsHeadersNotAuthorization(headersHeader[0]);
            return false;
        }

        return true;
    }

    private void SetCorsResponseHeaders(HttpRequest request, string origin)
    {
        LogSettingCorsHeaders();
        request.HttpContext.Response.Headers.Append("Access-Control-Allow-Origin", origin);
        request.HttpContext.Response.Headers.Append("Access-Control-Allow-Method", "GET");
        request.HttpContext.Response.Headers.Append("Access-Control-Allow-Headers", "Authorization");
        request.HttpContext.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
    }

    private bool TryGetAuthorizationHeader(IHeaderDictionary headers, out string authorization)
    {
        authorization = null;

        if (!headers.TryGetValue("Authorization", out var authorizationHeader))
        {
            LogAuthorizationMissing();
            return false;
        }

        if (authorizationHeader.Count != 1)
        {
            LogAuthorizationNotSingle(authorizationHeader);
            return false;
        }

        authorization = authorizationHeader[0];
        return true;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Request '{RequestPath}', CORS headers {{Origin: '{OriginHeader}'}} {{Headers: '{CorsHeadersHeader}'}} {{Method: '{CorsMethodHeader}'}}")]
    private partial void LogCorsRequest(string requestPath, StringValues originHeader, StringValues corsHeadersHeader, StringValues corsMethodHeader);

    [LoggerMessage(Level = LogLevel.Warning, Message = "This endpoint doesn't support CORS, not setting CORS headers")]
    private partial void LogNoCorsEndpoint();

    [LoggerMessage(Level = LogLevel.Warning, Message = "'Origin' header is missing, not setting CORS headers")]
    private partial void LogOriginMissing();

    [LoggerMessage(Level = LogLevel.Warning, Message = "Expected single origin header but found {OriginHeader}, not setting CORS headers")]
    private partial void LogOriginNotSingle(StringValues originHeader);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Origin {OriginHeader} does not match allowed origins {AllowedOrigins} or preview environment")]
    private partial void LogOriginNotAllowed(string originHeader, IEnumerable<string> allowedOrigins);

    [LoggerMessage(Level = LogLevel.Warning, Message = "'Access-Control-Request-Method' header is missing, not setting CORS headers")]
    private partial void LogCorsMethodMissing();

    [LoggerMessage(Level = LogLevel.Warning, Message = "Expected single CORS method header but found {CorsMethodHeader}, not setting CORS headers")]
    private partial void LogCorsMethodNotSingle(StringValues corsMethodHeader);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Expected 'GET' CORS method header but found '{CorsMethodHeader}', not setting CORS headers")]
    private partial void LogCorsMethodNotGet(string corsMethodHeader);

    [LoggerMessage(Level = LogLevel.Warning, Message = "'Access-Control-Request-Headers' header is missing, not setting CORS headers")]
    private partial void LogCorsHeadersMissing();

    [LoggerMessage(Level = LogLevel.Warning, Message = "Expected single CORS headers header but found {CorsHeadersHeader}, not setting CORS headers")]
    private partial void LogCorsHeadersNotSingle(StringValues corsHeadersHeader);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Expected 'authorization' CORS headers header but found '{CorsHeadersHeader}', not setting CORS headers")]
    private partial void LogCorsHeadersNotAuthorization(string corsHeadersHeader);

    [LoggerMessage(Level = LogLevel.Information, Message = "Setting CORS headers")]
    private partial void LogSettingCorsHeaders();

    [LoggerMessage(Level = LogLevel.Warning, Message = "'Authorization' header is missing")]
    private partial void LogAuthorizationMissing();

    [LoggerMessage(Level = LogLevel.Warning, Message = "Expected single authorization header but found {AuthorizationHeader}")]
    private partial void LogAuthorizationNotSingle(StringValues authorizationHeader);
}
