using Moq;
using ProductService.Application.DTOs;
using ProductService.Application.Exceptions;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using SutCategoryService = ProductService.Application.Services.CategoryService;

namespace ProductService.UnitTests.Services;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _categoryRepository = new();
    private readonly SutCategoryService _sut;

    public CategoryServiceTests()
    {
        _sut = new SutCategoryService(_categoryRepository.Object);
    }

    [Fact]
    public async Task CreateAsync_CreatesCategory_WhenNameIsUnique()
    {
        var dto = new CategoryCreateDto("Electronics");
        _categoryRepository.Setup(r => r.ExistsByNameAsync(dto.Name, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _sut.CreateAsync(dto);

        Assert.Equal("Electronics", result.Name);
        _categoryRepository.Verify(r => r.CreateAsync(It.Is<Category>(c => c.Name == "Electronics"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenNameAlreadyExists()
    {
        var dto = new CategoryCreateDto("Electronics");
        _categoryRepository.Setup(r => r.ExistsByNameAsync(dto.Name, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await Assert.ThrowsAsync<DuplicateCategoryException>(() => _sut.CreateAsync(dto));
        _categoryRepository.Verify(r => r.CreateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
