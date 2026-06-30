using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AI.PoweredEducation.Business.ArtificialIntelligence.Configuration;
using AI.PoweredEducation.Business.ArtificialIntelligence.Dtos;
using AI.PoweredEducation.Business.ArtificialIntelligence.Exceptions;
using AI.PoweredEducation.Business.ArtificialIntelligence.Interfaces;
using AI.PoweredEducation.Entity.Enums;
using Microsoft.Extensions.Options;

namespace AI.PoweredEducation.Business.ArtificialIntelligence.Providers;

public sealed class OpenAiProvider : IAiProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _httpClient;
    private readonly OpenAiOptions _options;

    public OpenAiProvider(HttpClient httpClient, IOptions<OpenAiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public string Name => "OpenAI";

    public Task<IReadOnlyCollection<GeneratedQuizTask>> GenerateQuizTasksAsync(
        AiGenerationContext context,
        int taskCount,
        CancellationToken cancellationToken = default)
    {
        var prompt = BuildBasePrompt(context, taskCount) +
            """

            Generate quiz tasks only.
            Each quiz task must contain:
            - one clear question
            - exactly four answer choices
            - one correct answer represented only as A, B, C, or D
            Avoid ambiguous questions and avoid answer choices like "all of the above".
            """;

        return GenerateAsync<QuizTasksEnvelope, GeneratedQuizTask>(
            prompt,
            "quiz_tasks_response",
            CreateQuizTasksSchema(),
            envelope => envelope.Tasks,
            cancellationToken);
    }

    public Task<IReadOnlyCollection<GeneratedQrCodeTask>> GenerateQrCodeTasksAsync(
        AiGenerationContext context,
        int taskCount,
        CancellationToken cancellationToken = default)
    {
        var prompt = BuildBasePrompt(context, taskCount) +
            """

            Generate QR code tasks only.
            Each QR code task must contain:
            - student-facing instructions for a physical classroom or school activity
            - a time limit in minutes
            Do not generate QR images. Do not generate GPS coordinates or physical target locations.
            """;

        return GenerateAsync<QrCodeTasksEnvelope, GeneratedQrCodeTask>(
            prompt,
            "qr_code_tasks_response",
            CreateQrCodeTasksSchema(),
            envelope => envelope.Tasks,
            cancellationToken);
    }

    private async Task<IReadOnlyCollection<TTask>> GenerateAsync<TEnvelope, TTask>(
        string userPrompt,
        string schemaName,
        object schema,
        Func<TEnvelope, IReadOnlyCollection<TTask>> extractTasks,
        CancellationToken cancellationToken)
    {
        EnsureConfigured();

        using var request = new HttpRequestMessage(HttpMethod.Post, "responses");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        request.Content = JsonContent.Create(
            new
            {
                model = _options.Model,
                input = new object[]
                {
                    new
                    {
                        role = "system",
                        content = "You generate concise, age-appropriate educational task drafts for teachers. Return only valid JSON matching the provided schema."
                    },
                    new
                    {
                        role = "user",
                        content = userPrompt
                    }
                },
                text = new
                {
                    format = new
                    {
                        type = "json_schema",
                        name = schemaName,
                        strict = true,
                        schema
                    }
                }
            },
            options: JsonOptions);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new AiProviderException(
                $"OpenAI request failed with status {(int)response.StatusCode}.");
        }

        var jsonPayload = ExtractOutputText(content);
        var envelope = JsonSerializer.Deserialize<TEnvelope>(jsonPayload, JsonOptions)
            ?? throw new AiProviderException("OpenAI returned an empty AI generation response.");

        var tasks = extractTasks(envelope);
        if (tasks.Count == 0)
        {
            throw new AiProviderException("OpenAI returned no generated tasks.");
        }

        return tasks;
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new AiProviderException("OpenAI API key is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_options.Model))
        {
            throw new AiProviderException("OpenAI model is not configured.");
        }
    }

    private static string BuildBasePrompt(AiGenerationContext context, int taskCount) =>
        $$"""
        Context:
        - Grade level: {{context.GradeLevel}}
        - Subject: {{context.Subject}}
        - Topic: {{context.Topic}}
        - Environment type: {{context.EnvironmentType}}
        - Expected student count: {{context.ExpectedStudentCount}}
        - Task count: {{taskCount}}

        Requirements:
        - Generate exactly {{taskCount}} tasks.
        - Keep content suitable for the grade level.
        - Teachers remain in control and may edit the generated draft.
        - Do not include explanations outside the JSON response.
        """;

    private static object CreateQuizTasksSchema() => new
    {
        type = "object",
        additionalProperties = false,
        properties = new
        {
            tasks = new
            {
                type = "array",
                minItems = 1,
                items = new
                {
                    type = "object",
                    additionalProperties = false,
                    properties = new
                    {
                        question = new { type = "string" },
                        optionA = new { type = "string" },
                        optionB = new { type = "string" },
                        optionC = new { type = "string" },
                        optionD = new { type = "string" },
                        correctAnswer = new
                        {
                            type = "string",
                            @enum = new[] { "A", "B", "C", "D" }
                        }
                    },
                    required = new[]
                    {
                        "question",
                        "optionA",
                        "optionB",
                        "optionC",
                        "optionD",
                        "correctAnswer"
                    }
                }
            }
        },
        required = new[] { "tasks" }
    };

    private static object CreateQrCodeTasksSchema() => new
    {
        type = "object",
        additionalProperties = false,
        properties = new
        {
            tasks = new
            {
                type = "array",
                minItems = 1,
                items = new
                {
                    type = "object",
                    additionalProperties = false,
                    properties = new
                    {
                        instructions = new { type = "string" },
                        timeLimitMinutes = new
                        {
                            type = "integer",
                            minimum = 1,
                            maximum = 60
                        }
                    },
                    required = new[] { "instructions", "timeLimitMinutes" }
                }
            }
        },
        required = new[] { "tasks" }
    };

    private static string ExtractOutputText(string responseJson)
    {
        using var document = JsonDocument.Parse(responseJson);
        var root = document.RootElement;

        if (root.TryGetProperty("output_text", out var outputText) &&
            outputText.ValueKind == JsonValueKind.String)
        {
            return outputText.GetString()!;
        }

        if (!root.TryGetProperty("output", out var output) ||
            output.ValueKind != JsonValueKind.Array)
        {
            throw new AiProviderException("OpenAI response does not contain output text.");
        }

        foreach (var outputItem in output.EnumerateArray())
        {
            if (!outputItem.TryGetProperty("content", out var content) ||
                content.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var contentItem in content.EnumerateArray())
            {
                if (contentItem.TryGetProperty("text", out var text) &&
                    text.ValueKind == JsonValueKind.String)
                {
                    return text.GetString()!;
                }
            }
        }

        throw new AiProviderException("OpenAI response output text is empty.");
    }

    private sealed record QuizTasksEnvelope(IReadOnlyCollection<GeneratedQuizTask> Tasks);
    private sealed record QrCodeTasksEnvelope(IReadOnlyCollection<GeneratedQrCodeTask> Tasks);
}
