namespace SmartExpenseSplitter.Api.Exceptions;

public class BadRequestException(string message) : ApiException(message, StatusCodes.Status400BadRequest);
