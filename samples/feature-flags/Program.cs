// ADR-0035 -- Feature flags demo
using Microsoft.AspNetCore.Http.HttpResults;
using FeatureFlags.Http;
using FeatureFlags.FlagStore;

var builder = WebApplication.CreateBuilder(args);

// --- Feature flag store (in-memory for demo; use Redis in production) ---
builder.Services.AddSingleton<IFeatureFlagStore>(sp =>
{
    var flags = new Dictionary<string, FeatureFlag>(StringComparer.OrdinalIgnoreCase)
