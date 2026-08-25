namespace ProductService.Application.Exceptions;

public class DuplicateCategoryException : Exception
{
    public DuplicateCategoryException(string name)
        : base($"A category named '{name}' already exists.")
    {
    }
}
