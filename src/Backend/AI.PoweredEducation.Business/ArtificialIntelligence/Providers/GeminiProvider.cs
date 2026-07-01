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

public sealed class GeminiProvider : IAiProvider
{
    private const int MaximumGenerationAttempts = 3;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _httpClient;
    private readonly GeminiOptions _options;

    public GeminiProvider(HttpClient httpClient, IOptions<GeminiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public string Name => "Gemini";

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
            - exactly four answer choices in properties named optionA, optionB, optionC, and optionD
            - one correct answer represented only as A, B, C, or D
            Do not return options as an array. Do not use property names like choices, answers, or option_a.
            Avoid ambiguous questions and avoid answer choices like "all of the above".
            """;

        return GenerateAsync(
            prompt,
            CreateQuizTasksSchema(),
            ParseQuizTasks,
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
            - exactly one short Turkish keyword related to the topic in the qrPayload property
            - brief student-facing instructions telling the student to scan the QR code for that keyword
            - a time limit in minutes
            The qrPayload value must be a single word when possible.
            Do not generate QR images. Do not generate GPS coordinates or physical target locations.
            """;

        return GenerateAsync(
            prompt,
            CreateQrCodeTasksSchema(),
            ParseQrCodeTasks,
            cancellationToken);
    }

    private async Task<IReadOnlyCollection<TTask>> GenerateAsync<TTask>(
        string prompt,
        object schema,
        Func<string, IReadOnlyCollection<TTask>> parseTasks,
        CancellationToken cancellationToken)
    {
        EnsureConfigured();

        AiProviderException? lastException = null;

        for (var attempt = 1; attempt <= MaximumGenerationAttempts; attempt++)
        {
            try
            {
                var content = await SendGenerationRequestAsync(prompt, schema, cancellationToken);
                var tasks = parseTasks(ExtractOutputText(content));
                if (tasks.Count == 0)
                {
                    throw new AiProviderException("Gemini returned no generated tasks.");
                }

                return tasks;
            }
            catch (AiProviderException exception) when (attempt < MaximumGenerationAttempts)
            {
                lastException = exception;
            }
        }

        throw lastException ?? new AiProviderException("Gemini generation failed.");
    }

    private async Task<string> SendGenerationRequestAsync(
        string prompt,
        object schema,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "interactions");
        request.Headers.Add("x-goog-api-key", _options.ApiKey);
        request.Content = JsonContent.Create(
            new
            {
                model = _options.Model,
                input = prompt,
                response_format = new
                {
                    type = "text",
                    mime_type = "application/json",
                    schema
                }
            },
            options: JsonOptions);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new AiProviderException(
                $"Gemini request failed with status {(int)response.StatusCode}.");
        }

        return content;
    }

    private static IReadOnlyCollection<GeneratedQuizTask> ParseQuizTasks(string jsonPayload)
    {
        using var document = JsonDocument.Parse(jsonPayload);
        var tasks = GetTasksArray(document.RootElement);

        return tasks
            .EnumerateArray()
            .Select(task => new GeneratedQuizTask(
                GetRequiredString(task, "question"),
                GetOptionString(task, 0, "optionA", "option_a", "a"),
                GetOptionString(task, 1, "optionB", "option_b", "b"),
                GetOptionString(task, 2, "optionC", "option_c", "c"),
                GetOptionString(task, 3, "optionD", "option_d", "d"),
                ParseCorrectAnswer(GetRequiredString(
                    task,
                    "correctAnswer",
                    "correct_answer",
                    "answer",
                    "correctOption",
                    "correct_option"))))
            .ToArray();
    }

    private static IReadOnlyCollection<GeneratedQrCodeTask> ParseQrCodeTasks(string jsonPayload)
    {
        using var document = JsonDocument.Parse(jsonPayload);
        var tasks = GetTasksArray(document.RootElement);

        return tasks
            .EnumerateArray()
            .Select(task => new GeneratedQrCodeTask(
                GetRequiredString(task, "instructions"),
                GetRequiredPositiveInt(task, "timeLimitMinutes", "time_limit_minutes"),
                GetRequiredString(task, "qrPayload", "qr_payload", "keyword", "word")))
            .ToArray();
    }

    private static JsonElement GetTasksArray(JsonElement root)
    {
        if (!root.TryGetProperty("tasks", out var tasks) ||
            tasks.ValueKind != JsonValueKind.Array)
        {
            throw new AiProviderException("Gemini response does not contain a tasks array.");
        }

        return tasks;
    }

    private static string GetRequiredString(JsonElement element, params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            if (TryGetProperty(element, propertyName, out var property) &&
                TryReadString(property, out var value))
            {
                return value;
            }
        }

        throw new AiProviderException(
            $"Gemini response is missing required property '{propertyNames[0]}'.");
    }

    private static int GetRequiredPositiveInt(JsonElement element, params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            if (TryGetProperty(element, propertyName, out var property) &&
                property.ValueKind == JsonValueKind.Number &&
                property.TryGetInt32(out var value) &&
                value > 0)
            {
                return value;
            }
        }

        throw new AiProviderException(
            $"Gemini response is missing required positive integer property '{propertyNames[0]}'.");
    }

    private static string GetOptionString(
        JsonElement element,
        int index,
        params string[] directPropertyNames)
    {
        foreach (var propertyName in directPropertyNames)
        {
            if (TryGetProperty(element, propertyName, out var property) &&
                TryReadString(property, out var value))
            {
                return value;
            }
        }

        foreach (var arrayPropertyName in new[]
        {
            "options",
            "choices",
            "answers",
            "answerOptions",
            "answer_options"
        })
        {
            if (!TryGetProperty(element, arrayPropertyName, out var arrayProperty) ||
                arrayProperty.ValueKind != JsonValueKind.Array ||
                arrayProperty.GetArrayLength() <= index)
            {
                continue;
            }

            var option = arrayProperty[index];
            if (TryReadString(option, out var optionValue))
            {
                return optionValue;
            }

            foreach (var nestedPropertyName in new[] { "text", "value", "label", "content" })
            {
                if (option.ValueKind == JsonValueKind.Object &&
                    TryGetProperty(option, nestedPropertyName, out var nestedProperty) &&
                    TryReadString(nestedProperty, out optionValue))
                {
                    return optionValue;
                }
            }
        }

        throw new AiProviderException(
            $"Gemini response is missing required property '{directPropertyNames[0]}'.");
    }

    private static bool TryGetProperty(
        JsonElement element,
        string propertyName,
        out JsonElement property)
    {
        if (element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty(propertyName, out property))
        {
            return true;
        }

        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var jsonProperty in element.EnumerateObject())
            {
                if (string.Equals(
                    jsonProperty.Name,
                    propertyName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    property = jsonProperty.Value;
                    return true;
                }
            }
        }

        property = default;
        return false;
    }

    private static bool TryReadString(JsonElement element, out string value)
    {
        if (element.ValueKind == JsonValueKind.String)
        {
            var stringValue = element.GetString();
            if (!string.IsNullOrWhiteSpace(stringValue))
            {
                value = stringValue.Trim();
                return true;
            }
        }

        if (element.ValueKind == JsonValueKind.Number)
        {
            value = element.GetRawText();
            return true;
        }

        value = string.Empty;
        return false;
    }

    private static QuizAnswerOption ParseCorrectAnswer(string value)
    {
        var normalized = value.Trim().Replace(" ", string.Empty, StringComparison.Ordinal);

        normalized = normalized.ToUpperInvariant() switch
        {
            "OPTIONA" or "ANSWERA" or "1" => "A",
            "OPTIONB" or "ANSWERB" or "2" => "B",
            "OPTIONC" or "ANSWERC" or "3" => "C",
            "OPTIOND" or "ANSWERD" or "4" => "D",
            _ => normalized
        };

        if (Enum.TryParse<QuizAnswerOption>(normalized, ignoreCase: true, out var answer))
        {
            return answer;
        }

        throw new AiProviderException("Gemini response contains an invalid correct answer.");
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new AiProviderException("Gemini API key is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_options.Model))
        {
            throw new AiProviderException("Gemini model is not configured.");
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
        - Return exactly one JSON object.
        - The JSON object must include a tasks array.
        - Do not include explanations outside the JSON response.
        """;

    private static object CreateQuizTasksSchema() => new
    {
        type = "object",
        properties = new
        {
            tasks = new
            {
                type = "array",
                items = new
                {
                    type = "object",
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
        properties = new
        {
            tasks = new
            {
                type = "array",
                items = new
                {
                    type = "object",
                    properties = new
                    {
                        instructions = new { type = "string" },
                        qrPayload = new { type = "string" },
                        timeLimitMinutes = new
                        {
                            type = "integer",
                            minimum = 1,
                            maximum = 60
                        }
                    },
                    required = new[] { "instructions", "qrPayload", "timeLimitMinutes" }
                }
            }
        },
        required = new[] { "tasks" }
    };

    private static string ExtractOutputText(string responseJson)
    {
        using var document = JsonDocument.Parse(responseJson);
        if (TryExtractJsonPayload(document.RootElement, out var payload))
        {
            return payload;
        }
        throw new AiProviderException("Gemini response output text is empty.");
    }

    private static bool TryExtractJsonPayload(JsonElement element, out string payload)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            if (element.TryGetProperty("tasks", out _))
            {
                payload = element.GetRawText();
                return true;
            }

            foreach (var property in element.EnumerateObject())
            {
                if (TryExtractJsonPayload(property.Value, out payload))
                {
                    return true;
                }
            }
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                if (TryExtractJsonPayload(item, out payload))
                {
                    return true;
                }
            }
        }

        if (element.ValueKind == JsonValueKind.String)
        {
            var text = element.GetString();
            if (!string.IsNullOrWhiteSpace(text) &&
                TryNormalizeJsonPayload(text, out payload))
            {
                return true;
            }
        }

        payload = string.Empty;
        return false;
    }

    private static bool TryNormalizeJsonPayload(string text, out string payload)
    {
        var trimmed = text.Trim();
        if (trimmed.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed[7..].Trim();
        }
        else if (trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            trimmed = trimmed[3..].Trim();
        }

        if (trimmed.EndsWith("```", StringComparison.Ordinal))
        {
            trimmed = trimmed[..^3].Trim();
        }

        try
        {
            using var document = JsonDocument.Parse(trimmed);
            if (document.RootElement.ValueKind == JsonValueKind.Object &&
                document.RootElement.TryGetProperty("tasks", out _))
            {
                payload = document.RootElement.GetRawText();
                return true;
            }
        }
        catch (JsonException)
        {
        }

        payload = string.Empty;
        return false;
    }

}
