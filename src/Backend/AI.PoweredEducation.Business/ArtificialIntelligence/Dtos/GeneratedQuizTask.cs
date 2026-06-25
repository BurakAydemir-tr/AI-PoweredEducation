using AI.PoweredEducation.Entity.Enums;

namespace AI.PoweredEducation.Business.ArtificialIntelligence.Dtos;

public sealed record GeneratedQuizTask(
    string Question,
    string OptionA,
    string OptionB,
    string OptionC,
    string OptionD,
    QuizAnswerOption CorrectAnswer);
