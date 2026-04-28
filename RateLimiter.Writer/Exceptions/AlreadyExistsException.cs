namespace RateLimiter.Writer.Exceptions;

[Serializable]
public class AlreadyExistsException : Exception
{
    public AlreadyExistsException() : base() { }
    public AlreadyExistsException(string message) : base(message) { }
    public AlreadyExistsException(string message, Exception inner) : base(message, inner) { }
}