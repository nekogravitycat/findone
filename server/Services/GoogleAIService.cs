﻿using Mscc.GenerativeAI;
using server.Models;
using System.Text.Json;

namespace server.Services
{
  public class GoogleAIService
  {
    private readonly string? _apiKey;
    private readonly GoogleAI _googleAI;
    private readonly GenerativeModel _model;
    private readonly GenerateContentConfig _generationConfig;
    private readonly string _systemInstruction =
        "You are an image recognition judge in a fun multiplayer game. " +
        "Each round, players submit images to match quirky prompts." +
        "Your task is to:\r\n" +
        "1. Evaluate whether the image matches the prompt.\r\n" +
        "2. Respond with:\r\n" +
        "   - A boolean indicating if the image matches the prompt(`true` or `false`)\r\n" +
        "   - A short, playful zh-tw comment(e.g., “恨帥潮” or “能拍出這張圖，也是挺有生活的”)\r\n" +
        "\r\n" +
        "Be lighthearted but clear." +
        "Responses should be fun and understandable by players." +
        "Avoid offensive or overly negative language.\r\n";

    private readonly string _responseSchema = @"
        {
          ""type"": ""object"",
          ""properties"": {
            ""match"": {
              ""type"": ""boolean""
            },
            ""comment"": {
              ""type"": ""string""
            }
          },
          ""required"": [""match"", ""comment""]
        }";

    public GoogleAIService(IConfiguration configuration)
    {
      _apiKey = configuration["APIKeys:GOOGLE_API_KEY"]
          ?? throw new ArgumentNullException("GOOGLE_API_KEY", "Missing GOOGLE_API_KEY in configuration.");
      _googleAI = new GoogleAI(_apiKey);
      _model = _googleAI.GenerativeModel(Model.Gemini20Flash, systemInstruction: new Content(_systemInstruction));
      _generationConfig = new GenerateContentConfig
      {
        ResponseMimeType = "application/json",
        ResponseSchema = _responseSchema
      };
    }

    public async Task<ImageResponse?> AnalyzeImage(string base64Image, string mimeType, string clientRequest)
    {
      GenerateContentRequest query = new GenerateContentRequest(clientRequest, _generationConfig);
      await query.AddMedia(base64Image, mimeType: mimeType);

      GenerateContentResponse response = await _model.GenerateContent(query);
      response.CheckResponse();
      return string.IsNullOrEmpty(response.Text) ? null : JsonSerializer.Deserialize<ImageResponse>(response.Text);
    }
  }
}
