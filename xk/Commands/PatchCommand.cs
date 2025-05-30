﻿using System.Collections.Generic;
using System.Net.Http.Headers;

using Cliffer;

using ParksComputing.XferKit.Api;
using ParksComputing.XferKit.Workspace;

namespace ParksComputing.XferKit.Cli.Commands;

[Command("patch", "Send a PATCH request with optional content.")]
[Argument(typeof(string), "payload", "Content to send with the PATCH request.")]
[Option(typeof(string), "--endpoint", "The endpoint to send the PATCH request to.", new[] { "-e" }, IsRequired = false)]
[Option(typeof(string), "--baseurl", "The base URL of the API.", new[] { "-b" }, IsRequired = false)]
[Option(typeof(IEnumerable<string>), "--headers", "Headers to include in the request.", new[] { "-h" }, AllowMultipleArgumentsPerToken = true, Arity = ArgumentArity.ZeroOrMore)]
internal class PatchCommand(XferKitApi xk) {
    public string ResponseContent { get; protected set; } = string.Empty;
    public int StatusCode { get; protected set; } = 0;
    public HttpResponseHeaders? Headers { get; protected set; }

    public int Execute(
        [ArgumentParam("payload")] string? payload,
        [OptionParam("--endpoint")] string endpoint,
        [OptionParam("--baseurl")] string? baseUrl,
        [OptionParam("--headers")] IEnumerable<string>? headers
    ) {
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var baseUri) || string.IsNullOrWhiteSpace(baseUri.Scheme)) {
            baseUrl ??= xk.ActiveWorkspace.BaseUrl;

            if (string.IsNullOrEmpty(baseUrl) || !Uri.TryCreate(new Uri(baseUrl), endpoint, out baseUri) || string.IsNullOrWhiteSpace(baseUri.Scheme)) {
                Console.Error.WriteLine($"{Constants.ErrorChar} Error: Invalid base URL: {baseUrl}");
                return Result.InvalidArguments;
            }
        }

        if (string.IsNullOrEmpty(payload) && Console.IsInputRedirected) {
            var input = Console.In.ReadToEnd();
            payload = input.Trim();
        }

        int result = Result.Success;

        try {
            var response = xk.Http.Patch(baseUri.ToString(), payload, headers);

            if (response is null) {
                Console.Error.WriteLine($"{Constants.ErrorChar} Error: No response received from {baseUri}");
                result = Result.Error;
            }
            else if (!response.IsSuccessStatusCode) {
                Console.Error.WriteLine($"{Constants.ErrorChar} {(int)response.StatusCode} {response.ReasonPhrase} at {baseUri}");
                result = Result.Error;
            }

            Headers = xk.Http.Headers;
            ResponseContent = xk.Http.ResponseContent;
            StatusCode = xk.Http.StatusCode;
        }
        catch (Exception ex) {
            Console.Error.WriteLine($"{Constants.ErrorChar} Error: {ex.Message}");
            result = Result.Error;
        }

        return result;
    }
}
