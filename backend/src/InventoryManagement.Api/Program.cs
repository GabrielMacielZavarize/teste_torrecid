using System.Text.Json.Serialization;
using InventoryManagement.Api.Authentication;
using InventoryManagement.Api.Authorization;
using InventoryManagement.Api.Documentation;
using InventoryManagement.Api.ErrorHandling;
using InventoryManagement.Application.Abstractions;
using InventoryManagement.Application.Auth;
using InventoryManagement.Application.Products;
using InventoryManagement.Application.Stock;
using InventoryManagement.Application.Users;
using InventoryManagement.Infrastructure;
using InventoryManagement.Infrastructure.Authentication;
using InventoryManagement.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("A connection string 'Default' não foi configurada.");
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("A seção 'Jwt' não foi configurada.");

builder.Services
    .AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecurityDocumentTransformer>());
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddInfrastructure(connectionString, jwtOptions);
builder.Services.AddJwtAuthentication(jwtOptions);
builder.Services.AddPermissionPolicies();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<StockService>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    await DatabaseInitializer.InitializeAsync(app.Services);

    app.MapOpenApi().AllowAnonymous();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "InventoryManagement API"));
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
