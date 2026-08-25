using Moq;
using ProductService.Application.DTOs;
using ProductService.Application.Exceptions;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using SutProductService = ProductService.Application.Services.ProductService;

namespace ProductService.UnitTests.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _productRepository = new();
    private readonly Mock<ICategoryRepository> _categoryRepository = new();
    private readonly SutProductService _sut;

    public ProductServiceTests()
    {
        _sut = new SutProductService(
            _productRepository.Object,
            _categoryRepository.Object,
            Mock.Of<IStockValidationStrategy>(),
            Mock.Of<IStockValidationStrategy>());
    }

    [Fact]
    public async Task CreateAsync_CreatesProduct_WhenCategoryExists()
    {
        var categoryId = Guid.NewGuid();
        var dto = new ProductCreateDto("Widget", "Description", 9.99m, 10, categoryId);
        _categoryRepository.Setup(r => r.ExistsAsync(categoryId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _sut.CreateAsync(dto);

        Assert.Equal("Widget", result.Name);
        _productRepository.Verify(r => r.CreateAsync(It.Is<Product>(p => p.Name == "Widget"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenCategoryDoesNotExist()
    {
        var categoryId = Guid.NewGuid();
        var dto = new ProductCreateDto("Widget", "Description", 9.99m, 10, categoryId);
        _categoryRepository.Setup(r => r.ExistsAsync(categoryId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await Assert.ThrowsAsync<InvalidCategoryReferenceException>(() => _sut.CreateAsync(dto));
        _productRepository.Verify(r => r.CreateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsProduct_WhenFound()
    {
        var product = new Product("Widget", null, 9.99m, 10, Guid.NewGuid());
        _productRepository.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var result = await _sut.GetByIdAsync(product.Id);

        Assert.Equal(product.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_Throws_WhenNotFound()
    {
        var id = Guid.NewGuid();
        _productRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(id));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesProduct_WhenProductAndCategoryExist()
    {
        var product = new Product("Widget", null, 9.99m, 10, Guid.NewGuid());
        var newCategoryId = Guid.NewGuid();
        var dto = new ProductUpdateDto("New Name", "New description", 19.99m, 5, newCategoryId);
        _productRepository.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _categoryRepository.Setup(r => r.ExistsAsync(newCategoryId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _sut.UpdateAsync(product.Id, dto);

        Assert.Equal("New Name", result.Name);
        Assert.Equal(19.99m, result.Price);
        _productRepository.Verify(r => r.UpdateAsync(product, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenProductDoesNotExist()
    {
        var id = Guid.NewGuid();
        var dto = new ProductUpdateDto("New Name", null, 19.99m, 5, Guid.NewGuid());
        _productRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.UpdateAsync(id, dto));
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenCategoryDoesNotExist()
    {
        var product = new Product("Widget", null, 9.99m, 10, Guid.NewGuid());
        var newCategoryId = Guid.NewGuid();
        var dto = new ProductUpdateDto("New Name", null, 19.99m, 5, newCategoryId);
        _productRepository.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _categoryRepository.Setup(r => r.ExistsAsync(newCategoryId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await Assert.ThrowsAsync<InvalidCategoryReferenceException>(() => _sut.UpdateAsync(product.Id, dto));
        _productRepository.Verify(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_DeletesProduct_WhenProductExists()
    {
        var id = Guid.NewGuid();
        _productRepository.Setup(r => r.ExistsAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await _sut.DeleteAsync(id);

        _productRepository.Verify(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_Throws_WhenProductDoesNotExist()
    {
        var id = Guid.NewGuid();
        _productRepository.Setup(r => r.ExistsAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteAsync(id));
        _productRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
