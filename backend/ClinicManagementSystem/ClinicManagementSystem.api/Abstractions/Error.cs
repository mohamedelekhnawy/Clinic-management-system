namespace ClinicManagementSystem.api.Abstractions
{
    public record Error(string Code, string Message, ErrorType Type)
    {
        public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);
    }

    public enum ErrorType
    {
        Failure = 0,
        Validation = 1,
        NotFound = 2,
        Conflict = 3
    }
}
