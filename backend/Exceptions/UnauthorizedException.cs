namespace backend.Exceptions;

public class UnauthorizedException(string message) : ApiException(message, StatusCodes.Status401Unauthorized);
