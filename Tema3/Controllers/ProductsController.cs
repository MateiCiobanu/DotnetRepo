using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Tema3.Application.Dtos;
using Tema3.Application.Handlers;
using Tema3.Application.Requests;
using Tema3.Domain.Entities;

namespace Tema3.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly ICreateProductHandler _create;

    public ProductsController(IMapper mapper, ICreateProductHandler create)
    {
        _mapper = mapper;
        _create = create;
    }

    // 1) 
    // Mapping direct Request -> Entity -> Dto (fără repo, fără cache, fără verificări)
    [HttpPost("profile-legacy")]
    public ActionResult<ProductProfileDto> CreateProfileLegacy([FromBody] CreateProductProfileRequest request)
    {
        var entity = _mapper.Map<Product>(request);
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;

        var dto = _mapper.Map<ProductProfileDto>(entity);
        return Ok(dto);
    }

    // 2)
    [HttpPost("profile")]
    public async Task<ActionResult<ProductProfileDto>> CreateProfile([FromBody] CreateProductProfileRequest request, CancellationToken ct)
    {
        try
        {
            var dto = await _create.Handle(request, ct);
            return Ok(dto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Unexpected error", detail = ex.Message });
        }
    }
}