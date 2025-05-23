using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Backend.DTO;
using Backend.Entidades.Entity;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public class ClienteLlm
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;

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

    public ClienteLlm(string apiKey,string model, HttpClient httpClient = null)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentNullException(nameof(apiKey));

        _apiKey = apiKey;
        _model = model;
        _httpClient = httpClient ?? new HttpClient();
    }



    public string GenerarJSON(string Instruccion, string NombreUsuario, decimal TotalGastos, decimal TotalEntradas, List<TransaccionDTO> transacciones,List<MensajeChat> mensajes)
    {
        // Obtener fechas de inicio y fin basadas en las transacciones
        DateTime? fechaInicio = transacciones?.Any() == true ? transacciones.Min(t => t.Fecha) : DateTime.Now.Date;
        DateTime? fechaFin = transacciones?.Any() == true ? transacciones.Max(t => t.Fecha) : DateTime.Now.Date;

        // Crear la estructura JSON
        var jsonStructure = new
        {
            contexto = Instruccion,
            resumen = "",
            datos = new
            {
                
                NombreUsuario,
                FechaInicio = fechaInicio?.ToString("yyyy-MM-dd"),
                FechaFin = fechaFin?.ToString("yyyy-MM-dd"),
                TotalGastos,
                TotalEntradas,
                Gastos = transacciones?.Select(t => new
                {
                    t.Tipo,
                    id = t.TransaccionID,
                    t.Monto,
                    Fecha = t.Fecha.ToString("yyyy-MM-dd"),
                    t.Titulo,
                    t.Descripcion,
                    t.Categoria
                }).ToArray() ?? new object[0]
            },
            mensajes = mensajes?.Select(m => new
            {
                role = m.Role,
                content = m.Content
            }).ToArray() ?? new object[0]
        };
        return JsonConvert.SerializeObject(jsonStructure, Formatting.Indented);
        
    }


   
    public async Task<RespuestaDTO> GenerarRespuestaAsync(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            return null;

        var requestUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

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
                string respuesta =  geminiResponse.Candidates[0].Content.Parts[0].Text;
                string CleanedJson = respuesta.Replace("```json", "").Replace("```", "").Trim();
                RespuestaDTO resultado = JsonConvert.DeserializeObject<RespuestaDTO>(CleanedJson);
                return resultado;
            }
            else if (geminiResponse?.PromptFeedback?.BlockReason != null)
            {
                Console.Error.WriteLine($"Content blocked by API: {geminiResponse.PromptFeedback.BlockReason}");
                return null;
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