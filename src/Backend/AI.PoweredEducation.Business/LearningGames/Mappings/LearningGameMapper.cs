using AI.PoweredEducation.Business.LearningGames.Dtos;
using AI.PoweredEducation.Entity.Entities;

namespace AI.PoweredEducation.Business.LearningGames.Mappings;

public static class LearningGameMapper
{
    public static LearningGame ToEntity(CreateLearningGameRequest request) => new()
    {
        GradeLevel = request.GradeLevel,
        Subject = request.Subject,
        Topic = request.Topic,
        EnvironmentType = request.EnvironmentType,
        ExpectedStudentCount = request.ExpectedStudentCount
    };

    public static void Apply(UpdateLearningGameRequest request, LearningGame game)
    {
        game.GradeLevel = request.GradeLevel;
        game.Subject = request.Subject;
        game.Topic = request.Topic;
        game.EnvironmentType = request.EnvironmentType;
        game.ExpectedStudentCount = request.ExpectedStudentCount;
    }

    public static LearningGameResponse ToResponse(LearningGame game) => new(
        game.Id,
        game.GradeLevel,
        game.Subject,
        game.Topic,
        game.EnvironmentType,
        game.ExpectedStudentCount,
        game.Status,
        game.GameCode,
        game.Tasks
            .OrderBy(task => task.Order)
            .Select(ToTaskResponse)
            .ToArray());

    public static LearningTaskResponse ToTaskResponse(LearningTask task) => task switch
    {
        QuizTask quiz => new LearningTaskResponse
        {
            Id = quiz.Id,
            TaskType = quiz.TaskType,
            Order = quiz.Order,
            Question = quiz.Question,
            OptionA = quiz.OptionA,
            OptionB = quiz.OptionB,
            OptionC = quiz.OptionC,
            OptionD = quiz.OptionD,
            CorrectAnswer = quiz.CorrectAnswer
        },
        QrCodeTask qr => new LearningTaskResponse
        {
            Id = qr.Id,
            TaskType = qr.TaskType,
            Order = qr.Order,
            Instructions = qr.Instructions,
            QrPayload = qr.QrPayload,
            TimeLimitMinutes = qr.TimeLimitMinutes
        },
        GpsTask gps => new LearningTaskResponse
        {
            Id = gps.Id,
            TaskType = gps.TaskType,
            Order = gps.Order,
            Instructions = gps.Instructions,
            TargetLatitude = gps.TargetLatitude,
            TargetLongitude = gps.TargetLongitude,
            GameAreaJson = gps.GameAreaJson,
            TimeLimitMinutes = gps.TimeLimitMinutes
        },
        _ => throw new InvalidOperationException($"Unsupported task type: {task.GetType().Name}.")
    };
}
