namespace SmartExpenseSplitter.Api.Exceptions;

public class ForbiddenException(string message) : ApiException(message, StatusCodes.Status403Forbidden);
