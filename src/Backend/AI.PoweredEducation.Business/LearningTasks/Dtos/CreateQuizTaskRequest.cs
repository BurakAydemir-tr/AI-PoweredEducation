using AI.PoweredEducation.Entity.Enums;

namespace AI.PoweredEducation.Business.LearningTasks.Dtos;

public sealed record CreateQuizTaskRequest(
    string Question,
    string OptionA,
    string OptionB,
    string OptionC,
    string OptionD,
    QuizAnswerOption CorrectAnswer);
