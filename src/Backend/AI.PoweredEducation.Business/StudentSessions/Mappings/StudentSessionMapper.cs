using AI.PoweredEducation.Business.StudentSessions.Dtos;
using AI.PoweredEducation.Entity.Entities;

namespace AI.PoweredEducation.Business.StudentSessions.Mappings;

public static class StudentSessionMapper
{
    public static StudentTaskResponse ToTaskResponse(LearningTask task) => task switch
    {
        QuizTask quiz => new StudentTaskResponse
        {
            Id = quiz.Id,
            TaskType = quiz.TaskType,
            Order = quiz.Order,
            Question = quiz.Question,
            OptionA = quiz.OptionA,
            OptionB = quiz.OptionB,
            OptionC = quiz.OptionC,
            OptionD = quiz.OptionD
        },
        QrCodeTask qr => new StudentTaskResponse
        {
            Id = qr.Id,
            TaskType = qr.TaskType,
            Order = qr.Order,
            Instructions = qr.Instructions,
            TimeLimitMinutes = qr.TimeLimitMinutes
        },
        GpsTask gps => new StudentTaskResponse
        {
            Id = gps.Id,
            TaskType = gps.TaskType,
            Order = gps.Order,
            Instructions = gps.Instructions,
            TargetLatitude = gps.TargetLatitude,
            TargetLongitude = gps.TargetLongitude,
            TimeLimitMinutes = gps.TimeLimitMinutes
        },
        _ => throw new InvalidOperationException($"Unsupported task type: {task.GetType().Name}.")
    };

    public static ResultResponse ToResultResponse(Result result) => new(
        result.Id,
        result.TotalScore,
        result.CompletionTime,
        result.CompletedTaskCount,
        result.UnfinishedTaskCount,
        result.TimedOutTaskCount,
        result.PlayedAt);
}
