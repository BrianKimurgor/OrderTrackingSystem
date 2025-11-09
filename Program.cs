using System.Diagnostics;
using OrderTrackingSystem.Data;
using OrderTrackingSystem.Services;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Version = "v1",
        Title = "Order Tracking System API",
        Description = "Real-time order tracking system with Kafka integration",
        Contact = new()
        {
            Name = "Support",
            Email = "support@ordertracking.com"
        }
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

builder.Services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
builder.Services.AddSingleton<IKafkaProducerService, KafkaProducerService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddHostedService<KafkaConsumerService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Tracking API v1");
    options.RoutePrefix = string.Empty;
});

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

var urls = builder.WebHost.GetSetting(WebHostDefaults.ServerUrlsKey)
           ?? $"http://localhost:{Environment.GetEnvironmentVariable("PORT") ?? "5000"}";

app.Logger.LogInformation("🚀 Order Tracking System started");
app.Logger.LogInformation("📚 Swagger UI available at: {Urls}", urls);

await app.RunAsync();
