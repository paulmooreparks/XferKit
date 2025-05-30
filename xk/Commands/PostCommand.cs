﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;

using ParksComputing.XferKit.Cli.Services;
using ParksComputing.XferKit.Workspace.Services;
using ParksComputing.XferKit.Http.Services;
using ParksComputing.XferKit.Api;
using ParksComputing.XferKit.Workspace;

namespace ParksComputing.XferKit.Cli.Commands;

[Command("post", "Send resources to the specified API endpoint via a POST request.")]
[Argument(typeof(string), "payload", "Content to send with the request. If input is redirected, content can also be read from standard input.")]
[Option(typeof(string), "--endpoint", "The endpoint to send the POST request to.", new[] { "-e" }, IsRequired = false, Arity = ArgumentArity.ExactlyOne)]
[Option(typeof(string), "--baseurl", "The base URL of the API to send HTTP requests to.", new[] { "-b" }, IsRequired = false)]
[Option(typeof(IEnumerable<string>), "--headers", "Headers to include in the request.", new[] { "-h" }, AllowMultipleArgumentsPerToken = true, Arity = ArgumentArity.ZeroOrMore)]
internal class PostCommand(
    XferKitApi xk
    ) 
{
    public string ResponseContent { get; protected set; } = string.Empty;
    public int StatusCode { get; protected set; } = 0;
    public System.Net.Http.Headers.HttpResponseHeaders? Headers { get; protected set; } = default;

    public int Execute(
        [ArgumentParam("payload")] string? payload,
        [OptionParam("--endpoint")] string? endpoint,
        [OptionParam("--baseurl")] string? baseUrl,
        [OptionParam("--headers")] IEnumerable<string> headers
        ) 
    {
        // Validate URL format
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var baseUri) || string.IsNullOrWhiteSpace(baseUri.Scheme)) {
            baseUrl ??= xk.ActiveWorkspace.BaseUrl;

            if (string.IsNullOrEmpty(baseUrl) || !Uri.TryCreate(new Uri(baseUrl), endpoint, out baseUri) || string.IsNullOrWhiteSpace(baseUri.Scheme)) {
                Console.Error.WriteLine($"{Constants.ErrorChar} Error: Invalid base URL: {baseUrl}");
                return Result.InvalidArguments;
            }
        }

        baseUrl = baseUri.ToString();

        if (string.IsNullOrEmpty(payload) && Console.IsInputRedirected) {
            var payloadString = Console.In.ReadToEnd();
            payload = payloadString.Trim();
        }

        int result = Result.Success;

        try {
            var response = xk.Http.Post(baseUrl, payload, headers);

            if (response is null) {
                Console.Error.WriteLine($"{Constants.ErrorChar} Error: No response received from {baseUrl}");
                result = Result.Error;
            }
            else if (!response.IsSuccessStatusCode) {
                Console.Error.WriteLine($"{Constants.ErrorChar} {(int)response.StatusCode} {response.ReasonPhrase} at {baseUrl}");
                result = Result.Error;
            }

            Headers = xk.Http.Headers;
            ResponseContent = xk.Http.ResponseContent;
            StatusCode = xk.Http.StatusCode;
            // List<Cookie> responseCookies = cookieContainer.GetCookies(baseUri).Cast<Cookie>().ToList();
        }
        catch (HttpRequestException ex) {
            Console.Error.WriteLine($"{Constants.ErrorChar} Error: HTTP request failed - {ex.Message}");
            return Result.Error;
        }

        return result;
    }
}
