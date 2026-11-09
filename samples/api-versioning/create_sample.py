import os

# api-versioning sample
os.makedirs('/Development/DotNet/Usman Architecture/samples/api-versioning/Controllers', exist_ok=True)
os.makedirs('/Development/DotNet/Usman Architecture/samples/api-versioning/Models', exist_ok=True)
os.makedirs('/Development/DotNet/Usman Architecture/samples/api-versioning/Data', exist_ok=True)

with open('/Development/DotNet/Usman Architecture/samples/api-versioning/api-versioning.csproj', 'w') as f:
    f.write('''<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="9.0.0" />
  </ItemGroup>

</Project>
''')

with open('/Development/DotNet/Usman Architecture/samples/api-versioning/Program.cs', 'w') as f:
    f.write('''// ADR-0025 — API Versioning Strategy (url-based versioning)

var builder = WebApplication.CreateBuilder(args);

// --- API Versioning configuration ---
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
''')

with open('/Development/DotNet/Usman Architecture/samples/api-versioning/Controllers/MainController.cs', 'w') as f:
    f.write('''// ADR-0025 — API versioning example

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
''')

with open('/Development/DotNet/Usman Architecture/samples/api-versioning/Models/TenantDto.cs', 'w') as f:
    f.write('''namespace ApiVersioning.Models;

public record TenantDto(string Id, string Name, string Status);

public record CreateTenantRequest(string Name, string? ContactEmail = null);
''')

print("api-versioning sample done")
