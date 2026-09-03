namespace ProductService.Application.Exceptions;

public class CategoryInUseException : Exception
{
    public CategoryInUseException(Guid categoryId)
        : base($"Category '{categoryId}' cannot be deleted because it still has associated products.")
    {
    }
}
