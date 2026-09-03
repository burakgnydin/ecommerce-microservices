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
    private readonly Mock<IProductRepository> _productRepository = new();
    private readonly SutCategoryService _sut;

    public CategoryServiceTests()
    {
        _sut = new SutCategoryService(_categoryRepository.Object, _productRepository.Object);
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

    [Fact]
    public async Task UpdateAsync_RenamesCategory_WhenNewNameIsAvailable()
    {
        var category = new Category("Electronics");
        _categoryRepository.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>())).ReturnsAsync(category);
        _categoryRepository.Setup(r => r.ExistsByNameAsync("Home Appliances", It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _sut.UpdateAsync(category.Id, new CategoryUpdateDto("Home Appliances"));

        Assert.Equal("Home Appliances", result.Name);
        _categoryRepository.Verify(r => r.UpdateAsync(category, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_DoesNotCheckDuplicate_WhenNameUnchanged()
    {
        var category = new Category("Electronics");
        _categoryRepository.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>())).ReturnsAsync(category);

        await _sut.UpdateAsync(category.Id, new CategoryUpdateDto("Electronics"));

        _categoryRepository.Verify(r => r.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _categoryRepository.Verify(r => r.UpdateAsync(category, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenCategoryDoesNotExist()
    {
        var id = Guid.NewGuid();
        _categoryRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Category?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.UpdateAsync(id, new CategoryUpdateDto("Electronics")));
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenNewNameIsAlreadyTaken()
    {
        var category = new Category("Electronics");
        _categoryRepository.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>())).ReturnsAsync(category);
        _categoryRepository.Setup(r => r.ExistsByNameAsync("Home Appliances", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await Assert.ThrowsAsync<DuplicateCategoryException>(() => _sut.UpdateAsync(category.Id, new CategoryUpdateDto("Home Appliances")));
        _categoryRepository.Verify(r => r.UpdateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_DeletesCategory_WhenNoProductsReferenceIt()
    {
        var id = Guid.NewGuid();
        _categoryRepository.Setup(r => r.ExistsAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _productRepository.Setup(r => r.ExistsByCategoryIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await _sut.DeleteAsync(id);

        _categoryRepository.Verify(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_Throws_WhenCategoryDoesNotExist()
    {
        var id = Guid.NewGuid();
        _categoryRepository.Setup(r => r.ExistsAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteAsync(id));
    }

    [Fact]
    public async Task DeleteAsync_Throws_WhenCategoryHasProducts()
    {
        var id = Guid.NewGuid();
        _categoryRepository.Setup(r => r.ExistsAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _productRepository.Setup(r => r.ExistsByCategoryIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await Assert.ThrowsAsync<CategoryInUseException>(() => _sut.DeleteAsync(id));
        _categoryRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
