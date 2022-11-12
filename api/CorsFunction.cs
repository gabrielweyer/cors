using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace api;

public class CorsFunction
{
    private readonly ILogger<CorsFunction> _logger;
    private static readonly List<string> AllowedOrigins = new() { "http://localhost:4200", "https://salmon-pond-0870a1e00.2.azurestaticapps.net" };
    private static readonly Regex PreviewEnvironment =
        new Regex("https:\\/\\/salmon-pond-0870a1e00-\\d+\\.eastasia\\.2\\.azurestaticapps\\.net");

    public CorsFunction(ILogger<CorsFunction> logger)
    {
        _logger = logger;
    }

    [FunctionName("NoCorsOptions")]
    public IActionResult RunNoCorsOptions(
        [HttpTrigger(AuthorizationLevel.Anonymous, "options", Route = "no-cors")]
        HttpRequest request)
    {
        LogCors(request.Path, request.Headers);

        _logger.LogWarning("This endpoint doesn't support CORS, not setting CORS headers");

        return new OkResult();
    }

    [FunctionName("NoCorsGet")]
    public IActionResult RunNoCorsGet(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "no-cors")]
        HttpRequest request)
    {
        return new OkResult();
    }

    [FunctionName("CorsNoCacheOptions")]
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

    [FunctionName("CorsNoCacheGet")]
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

        request.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", origin);
        return new OkObjectResult(new { Authorization = authorization });
    }

    [FunctionName("CorsCacheOptions")]
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
        request.HttpContext.Response.Headers.Add("Access-Control-Max-Age", "600");
        return new OkResult();
    }

    [FunctionName("CorsCacheGet")]
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

        request.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", origin);
        return new OkObjectResult(new { Authorization = authorization });
    }

    private void LogCors(string path, IHeaderDictionary headers)
    {
        _logger.LogInformation(
            "Request '{RequestPath}', CORS headers {{Origin: '{OriginHeader}'}} {{Headers: '{CorsHeadersHeader}'}} {{Method: '{CorsMethodHeader}'}}",
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
            _logger.LogWarning("'Origin' header is missing, not setting CORS headers");
            return false;
        }

        if (originHeader.Count != 1)
        {
            _logger.LogWarning(
                "Expected single origin header but found {OriginHeader}, not setting CORS headers",
                originHeader);
            return false;
        }

        var tentativeOrigin = originHeader[0];

        if (!AllowedOrigins.Contains(tentativeOrigin) && !PreviewEnvironment.IsMatch(tentativeOrigin))
        {
            _logger.LogWarning(
                "Origin {OriginHeader} does not match allowed origins {AllowedOrigins} or preview environment",
                tentativeOrigin,
                AllowedOrigins);
            return false;
        }

        origin = tentativeOrigin;
        return true;
    }

    private bool ValidateCorsMethodHeader(IHeaderDictionary headers)
    {
        if (!headers.TryGetValue("Access-Control-Request-Method", out var methodHeader))
        {
            _logger.LogWarning("'Access-Control-Request-Method' header is missing, not setting CORS headers");
            return false;
        }

        if (methodHeader.Count != 1)
        {
            _logger.LogWarning(
                "Expected single CORS method header but found {CorsMethodHeader}, not setting CORS headers",
                methodHeader);
            return false;
        }

        if (!"GET".Equals(methodHeader[0], StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Expected 'GET' CORS method header but found '{CorsMethodHeader}', not setting CORS headers",
                methodHeader[0]);
            return false;
        }

        return true;
    }

    private bool ValidateCorsHeadersHeader(IHeaderDictionary headers)
    {
        if (!headers.TryGetValue("Access-Control-Request-Headers", out var headersHeader))
        {
            _logger.LogWarning("'Access-Control-Request-Headers' header is missing, not setting CORS headers");
            return false;
        }

        if (headersHeader.Count != 1)
        {
            _logger.LogWarning(
                "Expected single CORS headers header but found {CorsHeadersHeader}, not setting CORS headers",
                headersHeader);
            return false;
        }

        if (!"authorization".Equals(headersHeader[0], StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Expected 'authorization' CORS headers header but found '{CorsHeadersHeader}', not setting CORS headers",
                headersHeader[0]);
            return false;
        }

        return true;
    }

    private void SetCorsResponseHeaders(HttpRequest request, string origin)
    {
        _logger.LogInformation("Setting CORS headers");
        request.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", origin);
        request.HttpContext.Response.Headers.Add("Access-Control-Allow-Method", "GET");
        request.HttpContext.Response.Headers.Add("Access-Control-Allow-Headers", "Authorization");
        request.HttpContext.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
    }

    private bool TryGetAuthorizationHeader(IHeaderDictionary headers, out string authorization)
    {
        authorization = null;

        if (!headers.TryGetValue("Authorization", out var authorizationHeader))
        {
            _logger.LogWarning("'Authorization' header is missing");
            return false;
        }

        if (authorizationHeader.Count != 1)
        {
            _logger.LogWarning(
                "Expected single authorization header but found {AuthorizationHeader}",
                authorizationHeader);
            return false;
        }

        authorization = authorizationHeader[0];
        return true;
    }
}
