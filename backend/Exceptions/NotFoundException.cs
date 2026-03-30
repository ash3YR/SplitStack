namespace backend.Exceptions;

public class NotFoundException(string message) : ApiException(message, StatusCodes.Status404NotFound);
