using MiddlewareEngine.Models;
using System.Text;
using System.Text.Json;

namespace MiddlewareEngine.Executors;

public class RestApiExecutor : IFunctionExecutor
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<RestApiExecutor> _logger;

    public RestApiExecutor(IHttpClientFactory httpClientFactory, ILogger<RestApiExecutor> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<FunctionExecutionResponse> ExecuteAsync(
        FunctionDefinition definition,
        Dictionary<string, object>? parameters)
    {
        var response = new FunctionExecutionResponse
        {
            FunctionId = definition.FunctionId
        };

        try
        {
            var config = definition.ExecutionConfig;
            if (string.IsNullOrEmpty(config.Url) || string.IsNullOrEmpty(config.HttpMethod))
            {
                throw new InvalidOperationException("REST API configuration is incomplete");
            }

            var client = _httpClientFactory.CreateClient();
            
            // Set timeout if specified
            if (config.Timeout.HasValue)
            {
                client.Timeout = TimeSpan.FromSeconds(config.Timeout.Value);
            }

            // Add custom headers
            if (config.Headers != null)
            {
                foreach (var header in config.Headers)
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }

            // Build URL with query parameters if GET
            var url = config.Url;
            HttpResponseMessage httpResponse;

            switch (config.HttpMethod.ToUpper())
            {
                case "GET":
                    if (parameters != null && parameters.Any())
                    {
                        var queryString = string.Join("&", parameters.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value?.ToString() ?? "")}"));
                        url = $"{url}?{queryString}";
                    }
                    httpResponse = await client.GetAsync(url);
                    break;

                case "POST":
                    var postContent = new StringContent(
                        JsonSerializer.Serialize(parameters ?? new Dictionary<string, object>()),
                        Encoding.UTF8,
                        "application/json");
                    httpResponse = await client.PostAsync(url, postContent);
                    break;

                case "PUT":
                    var putContent = new StringContent(
                        JsonSerializer.Serialize(parameters ?? new Dictionary<string, object>()),
                        Encoding.UTF8,
                        "application/json");
                    httpResponse = await client.PutAsync(url, putContent);
                    break;

                case "DELETE":
                    httpResponse = await client.DeleteAsync(url);
                    break;

                default:
                    throw new NotSupportedException($"HTTP method '{config.HttpMethod}' is not supported");
            }

            var result = await httpResponse.Content.ReadAsStringAsync();
            
            if (httpResponse.IsSuccessStatusCode)
            {
                response.Success = true;
                response.Result = result;
            }
            else
            {
                response.Success = false;
                response.ErrorMessage = $"HTTP {(int)httpResponse.StatusCode}: {result}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing REST API function: {FunctionId}", definition.FunctionId);
            response.Success = false;
            response.ErrorMessage = ex.Message;
        }

        return response;
    }
}
