namespace AI.PoweredEducation.Business.Results.Dtos;

public sealed record GameResultsResponse(
    Guid LearningGameId,
    int ExpectedStudents,
    int JoinedStudents,
    int CompletedStudents,
    IReadOnlyCollection<StudentResultResponse> StudentResults);
