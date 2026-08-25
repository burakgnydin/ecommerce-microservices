using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Common;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;

namespace ProductService.Api.Controllers;

/// <summary>
/// Manages products: creation, retrieval, updates and deletion.
/// </summary>
[ApiController]
[Route("api/products")]
[Tags("Products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Returns a paged list of products.
    /// </summary>
    /// <param name="pageNumber">Page number, starting at 1.</param>
    /// <param name="pageSize">Number of items per page (1-100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet]
    [ProducesResponseType<PagedResult<ProductResponseDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ProductResponseDto>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _productService.GetAllAsync(pageNumber, pageSize, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Returns a single product by id.
    /// </summary>
    /// <param name="id">Product id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ProductResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _productService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="dto">Product data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPost]
    [ProducesResponseType<ProductResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductResponseDto>> Create(ProductCreateDto dto, CancellationToken cancellationToken)
    {
        var result = await _productService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="id">Product id.</param>
    /// <param name="dto">Updated product data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<ProductResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponseDto>> Update(Guid id, ProductUpdateDto dto, CancellationToken cancellationToken)
    {
        var result = await _productService.UpdateAsync(id, dto, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Deletes a product.
    /// </summary>
    /// <param name="id">Product id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _productService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
