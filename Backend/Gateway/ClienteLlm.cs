using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public class ClienteLlm
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private const string DefaultModel = "gemini-2.5-flash-preview-04-17";

    private class GeminiRequestPayload
    {
        public List<ContentItem> Contents { get; set; } = new List<ContentItem>();
    }

    private class ContentItem
    {
        public List<PartItem> Parts { get; set; } = new List<PartItem>();
    }

    private class PartItem
    {
        public string Text { get; set; } = string.Empty;
    }

    // Response classes
    private class GeminiResponsePayload
    {
        public List<CandidateItem> Candidates { get; set; }
        public PromptFeedbackItem PromptFeedback { get; set; }
    }

    private class CandidateItem
    {
        public ContentItem Content { get; set; }
        public string FinishReason { get; set; }
    }

    private class PromptFeedbackItem
    {
        public string BlockReason { get; set; }
    }

    // JSON settings
    private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        NullValueHandling = NullValueHandling.Ignore
    };

    public ClienteLlm(string apiKey, HttpClient httpClient = null)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentNullException(nameof(apiKey));

        _apiKey = apiKey;
        _httpClient = httpClient ?? new HttpClient();
    }

    private async Task<string> GenerateTextAsync(string prompt, string modelName = DefaultModel)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            return null;

        var requestUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent?key={_apiKey}";

        var payload = new GeminiRequestPayload
        {
            Contents = new List<ContentItem>
            {
                new ContentItem
                {
                    Parts = new List<PartItem> { new PartItem { Text = prompt } }
                }
            }
        };

        try
        {
            var jsonPayload = JsonConvert.SerializeObject(payload, _jsonSettings);
            var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(requestUrl, httpContent);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.Error.WriteLine($"API Error: {response.StatusCode} - {responseString}");
                return null;
            }

            var geminiResponse = JsonConvert.DeserializeObject<GeminiResponsePayload>(responseString, _jsonSettings);

            if (geminiResponse?.Candidates != null && geminiResponse.Candidates.Count > 0 &&
                geminiResponse.Candidates[0].Content?.Parts != null && geminiResponse.Candidates[0].Content.Parts.Count > 0)
            {
                return geminiResponse.Candidates[0].Content.Parts[0].Text;
            }
            else if (geminiResponse?.PromptFeedback?.BlockReason != null)
            {
                Console.Error.WriteLine($"Content blocked by API: {geminiResponse.PromptFeedback.BlockReason}");
                return $"[Content Blocked: {geminiResponse.PromptFeedback.BlockReason}]";
            }
            else
            {
                Console.Error.WriteLine("No text content found in the response.");
                return null;
            }
        }
        catch (JsonException jsonEx)
        {
            Console.Error.WriteLine($"JSON Deserialization Error: {jsonEx.Message}");
            return null;
        }
        catch (HttpRequestException httpEx)
        {
            Console.Error.WriteLine($"HTTP Request Error: {httpEx.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"An unexpected error occurred: {ex.Message}");
            return null;
        }
    }
}