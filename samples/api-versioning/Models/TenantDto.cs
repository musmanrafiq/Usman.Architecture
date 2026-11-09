namespace ApiVersioning.Models;

public record TenantDto(string Id, string Name, string Status);

public record CreateTenantRequest(string Name, string? ContactEmail = null);
