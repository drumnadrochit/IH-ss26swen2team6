namespace TourPlanner.BL.Exceptions;

// The BL layer defines its own exception vocabulary instead of leaking generic
// .NET/EF/HTTP exceptions to its callers. Controllers catch these and translate
// them to the appropriate HTTP status code.

public class EntityNotFoundException(string message) : Exception(message);

public class ForbiddenAccessException(string message) : Exception(message);

public class DomainValidationException(string message) : Exception(message);

public class ConflictException(string message) : Exception(message);

public class InvalidCredentialsException(string message) : Exception(message);
