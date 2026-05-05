using CoffeeShop.PaymentApi.Repositories;
using CoffeeShop.PaymentApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // PayOS yêu cầu camelCase cho JSON
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
// Configure CORS để WPF client có thể gọi API
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost", "app://wpf" };
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWpfClient", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .SetIsOriginAllowed(origin => 
                  origin.StartsWith("http://localhost") || 
                  origin.StartsWith("https://localhost") ||
                  origin.StartsWith("app://"));
    });
});

// Configure HttpClient cho PaymentGatewayService
builder.Services.AddHttpClient<IPaymentGatewayService, PayOsPaymentGatewayService>();

// Register repositories và services
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentGatewayService, PayOsPaymentGatewayService>();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CoffeeShop Payment API v1");
        c.RoutePrefix = string.Empty; // Swagger UI tại root URL
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowWpfClient");

app.UseAuthorization();

app.MapControllers();

// Log startup info
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("CoffeeShop Payment API started");
logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
logger.LogInformation("Swagger UI: {Url}", app.Environment.IsDevelopment() ? "https://localhost:5001" : "Disabled");

app.Run();
