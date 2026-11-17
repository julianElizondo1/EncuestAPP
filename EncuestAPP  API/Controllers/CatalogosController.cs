using EncuestAPP__API.Data;
using EncuestAPP__API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EncuestAPP__API.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/catalogos")]
public sealed class CatalogosController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet("estados")]
    public async Task<ActionResult<IReadOnlyList<CatalogoDto>>> Estados(CancellationToken cancellationToken) =>
        Ok(await dbContext.EstadosEncuesta.AsNoTracking().OrderBy(x => x.Id)
            .Select(x => new CatalogoDto(x.Id, x.Nombre, null)).ToListAsync(cancellationToken));

    [HttpGet("categorias")]
    public async Task<ActionResult<IReadOnlyList<CatalogoDto>>> Categorias(CancellationToken cancellationToken) =>
        Ok(await dbContext.CategoriasEncuesta.AsNoTracking().OrderBy(x => x.Id)
            .Select(x => new CatalogoDto(x.Id, x.Nombre, x.Descripcion)).ToListAsync(cancellationToken));

    [HttpGet("tipos-pregunta")]
    public async Task<ActionResult<IReadOnlyList<CatalogoDto>>> TiposPregunta(CancellationToken cancellationToken) =>
        Ok(await dbContext.TiposPregunta.AsNoTracking().OrderBy(x => x.Id)
            .Select(x => new CatalogoDto(x.Id, x.Nombre, null)).ToListAsync(cancellationToken));
}
