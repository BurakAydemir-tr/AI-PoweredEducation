using AI.PoweredEducation.Entity.Enums;

namespace AI.PoweredEducation.Entity.Entities;

public sealed class QuizTask : LearningTask
{
    public QuizTask()
        : base(LearningTaskType.Quiz)
    {
    }

    public string Question { get; set; } = string.Empty;

    public string OptionA { get; set; } = string.Empty;

    public string OptionB { get; set; } = string.Empty;

    public string OptionC { get; set; } = string.Empty;

    public string OptionD { get; set; } = string.Empty;

    public QuizAnswerOption CorrectAnswer { get; set; }
}
