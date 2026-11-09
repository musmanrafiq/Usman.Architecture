// ADR-0025 — API versioning example

using Microsoft.AspNetCore.Mvc;

namespace ApiVersioning.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion(1.0)]
public class TenantsController : ControllerBase
{
    private readonly ILogger<TenantsController> _logger;

    public TenantsController(ILogger<TenantsController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get tenant by ID (v1).
    /// </summary>
    [HttpGet("{tenantId}")]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetTenant(string tenantId)
    {
        _logger.LogInformation("Getting tenant {TenantId} (v1)", tenantId);
        return Ok(new TenantDto { Id = tenantId, Name = "Acme Corp", Status = "Active" });
    }

    /// <summary>
    /// Create a new tenant (v1).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status201Created)]
    public IActionResult CreateTenant([FromBody] CreateTenantRequest request)
    {
        _logger.LogInformation("Creating tenant {Name}" , request.Name);
        return CreatedAtAction(nameof(GetTenant), new { tenantId = request.Name }, new TenantDto { Id = request.Name, Name = request.Name, Status = "Active" });
    }
}
