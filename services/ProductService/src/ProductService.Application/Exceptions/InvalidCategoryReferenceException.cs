namespace ProductService.Application.Exceptions;

public class InvalidCategoryReferenceException : Exception
{
    public InvalidCategoryReferenceException(Guid categoryId)
        : base($"Category '{categoryId}' does not exist.")
    {
    }
}
