using GameDBWebApi.Data;
using GameDBWebApi.Repositories;
using GameDBWebApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using GameDBWebApi.Constants;
using Microsoft.VisualBasic;
using GameDBWebApi.Configuration;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException(ErrorMessages.Auth.JwtSecretKeyNotConfigured);

var rawgApiKey = builder.Configuration["RawgApiKey"] ?? throw new InvalidOperationException(ErrorMessages.External.RawgApiKeyNotConfigured);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "GameVaultApi",
        ValidAudience = jwtSettings["Audience"] ?? "GameVaultUsers",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

// Add controllers and register a global model validation filter so validation
// errors are converted to BadRequestException and handled by GlobalExceptionMiddleware
builder.Services
    .AddControllers(options =>
    {
        options.Filters.Add<GameDBWebApi.Filters.ValidateModelFilter>();
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // Suppress the default ASP.NET Core behavior that auto-returns 400 for invalid ModelState
        // so our ValidateModelFilter can throw BadRequestException and the GlobalExceptionMiddleware
        // will produce a consistent error response shape.
        options.SuppressModelStateInvalidFilter = true;
    });

// FluentValidation registration: scans this assembly for validator classes
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddOptions<EmailSettings>()
    .Bind(builder.Configuration.GetSection(EmailSettings.SectionName))
    .Validate(settings => !string.IsNullOrWhiteSpace(settings.SmtpHost), "EmailSettings:SmtpHost is required.")
    .Validate(settings => !string.IsNullOrWhiteSpace(settings.Username), "EmailSettings:Username is required.")
    .Validate(settings => !string.IsNullOrWhiteSpace(settings.Password), "EmailSettings:Password is required.")
    .Validate(settings => !string.IsNullOrWhiteSpace(settings.FromAddress), "EmailSettings:FromAddress is required.")
    .Validate(settings => !string.IsNullOrWhiteSpace(settings.VerificationUrl), "EmailSettings:VerificationUrl is required.")
    .Validate(settings => settings.VerificationTokenLifetimeMinutes > 0, "EmailSettings:VerificationTokenLifetimeMinutes must be greater than zero.")
    .ValidateOnStart();

builder.Host.UseSerilog((context, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    // Add the "Authorize" button to Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Paste your JWT Token here in this format: 'Bearer {your_token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Apply the security requirement to all endpoints
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
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



//builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, MailKitEmailService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services
    .AddHttpClient<IGameCatalogService, RawgApiService>(client =>
    {
        client.BaseAddress = new Uri("https://api.rawg.io/");
    })
    .AddStandardResilienceHandler(options =>
    {
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
        options.Retry.MaxRetryAttempts = 3;
        options.Retry.Delay = TimeSpan.FromMilliseconds(200);
        options.Retry.BackoffType = DelayBackoffType.Exponential;
        options.Retry.UseJitter = true;

        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
        options.CircuitBreaker.FailureRatio = 0.5;
        options.CircuitBreaker.MinimumThroughput = 5;
        options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);
    });
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IUserListService, UserListService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GameDBWebApi.MiddleWares.GlobalExceptionMiddleware>();

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
