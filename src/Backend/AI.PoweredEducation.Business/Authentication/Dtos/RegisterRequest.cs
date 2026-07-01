namespace AI.PoweredEducation.Business.Authentication.Dtos;

public sealed record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password);
