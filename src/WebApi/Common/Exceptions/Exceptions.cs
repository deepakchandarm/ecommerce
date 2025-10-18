namespace WebApi.Common.Exceptions
{
    public class ResourceNotFoundException : Exception
    {
        public ResourceNotFoundException(string message) : base(message) { }
    }

    public class ProductNotPresentException : Exception
    {
        public ProductNotPresentException(string message) : base(message) { }
    }

    public class JwtAuthenticationException : Exception
    {
        public JwtAuthenticationException(string message) : base(message) { }
    }

    public class CategoryNotPresentException : Exception
    {
        public CategoryNotPresentException(string message) : base(message) { }
    }

    public class AlreadyExistException : Exception
    {
        public AlreadyExistException(string message) : base(message) { }
    }

    public class UserAlreadyExistException : Exception
    {
        public UserAlreadyExistException(string message) : base(message) { }
    }

    public class InvalidPasswordException : Exception
    {
        public InvalidPasswordException(string message) : base(message) { }
    }
}
