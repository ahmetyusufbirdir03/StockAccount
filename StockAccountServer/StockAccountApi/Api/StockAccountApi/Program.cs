using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using DotNetEnv;
using StockAccountInfrastructure;
using StockAccountApplication;
using StockAccountContracts;

var builder = WebApplication.CreateBuilder(args);


Console.WriteLine("========== APPLICATION STARTING ==========");
//Console.WriteLine($"Current Directory: {Directory.GetCurrentDirectory()}");

var currentDir = Directory.GetCurrentDirectory();

var apiDir = Directory.GetParent(currentDir)?.FullName;

var solutionRoot = Directory.GetParent(apiDir ?? "")?.FullName;

var envPath = Path.Combine(solutionRoot ?? "", ".env");

if (File.Exists(envPath))
{
    Env.Load(envPath);
}
else
{
    Console.WriteLine(".env NOT FOUND!");
}


builder.Logging.ClearProviders();



Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()

    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)

    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)

    .WriteTo.Console(
        theme: AnsiConsoleTheme.Code,
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} <{SourceContext}>{NewLine}{Exception}"
    )
    .CreateLogger();

builder.Host.UseSerilog();

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddContracts(builder.Configuration);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Stock/Account",
        Version = "v1",
        Description = "Stock/Account Client"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme()
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();  

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
