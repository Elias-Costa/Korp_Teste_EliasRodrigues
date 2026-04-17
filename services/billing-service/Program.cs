using System.Text.Json.Serialization;
using BillingService.Data;
using BillingService.Middleware;
using BillingService.Options;
using BillingService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:4200"];

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddDbContext<BillingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.Configure<InventoryServiceOptions>(
    builder.Configuration.GetSection(InventoryServiceOptions.SectionName));
builder.Services.AddHttpClient<IInventoryApiClient, InventoryApiClient>((serviceProvider, client) =>
{
    var options = serviceProvider
        .GetRequiredService<IOptions<InventoryServiceOptions>>()
        .Value;

    client.BaseAddress = new Uri(options.InventoryServiceBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddScoped<InvoiceService>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("Frontend");

app.MapHealthChecks("/health");
app.MapControllers();

await app.InitializeDatabaseAsync();

app.Run();
