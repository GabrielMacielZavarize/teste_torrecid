using InventoryManagement.Api.Authorization;
using InventoryManagement.Application.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Api.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(ProductService productService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.ProductsView)]
    public Task<IReadOnlyList<ProductDto>> List(CancellationToken cancellationToken)
    {
        return productService.ListAsync(cancellationToken);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.ProductsView)]
    public Task<ProductDto> GetById(Guid id, CancellationToken cancellationToken)
    {
        return productService.GetByIdAsync(id, cancellationToken);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.ProductsManage)]
    public async Task<ActionResult<ProductDto>> Create(
        CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = await productService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.ProductsManage)]
    public Task<ProductDto> Update(Guid id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        return productService.UpdateAsync(id, request, cancellationToken);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.ProductsManage)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await productService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
