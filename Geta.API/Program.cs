using System.Text;
using Geta.API.Data;
using Geta.API.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Configura��o do CORS (Cross-Origin Resource Sharing) ---
// Permite que o frontend Angular acesse a API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // URL do seu app Angular
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Importante para SignalR com autentica��o
    });
});


// --- 2. Configura��o do Banco de Dados (MySQL com EF Core) ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);


// --- 3. Configura��o da Autentica��o JWT ---
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
{
    throw new InvalidOperationException("A chave JWT deve ser definida em appsettings.json e ter pelo menos 32 caracteres.");
}

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
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };

    // Configura��o para SignalR usar o token JWT do query string para autentica��o
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chathub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});


// --- 4. Registro de Outros Servi�os ---
builder.Services.AddControllers();
builder.Services.AddSignalR(); // Adiciona os servi�os do SignalR

// Configura��o do Swagger/OpenAPI para documenta��o e teste da API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Adiciona a defini��o de seguran�a para JWT no Swagger UI
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Por favor, insira 'Bearer' seguido de um espa�o e o token JWT",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{}
        }
    });
});

// --- Constru��o da Aplica��o ---
var app = builder.Build();

// Configura��o do pipeline de requisi��es HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Aplica a pol�tica de CORS definida acima
app.UseCors("AllowAngularApp");

// Habilita a autentica��o
app.UseAuthentication();
app.UseAuthorization();

// --- 5. Mapeamento de Endpoints ---
// Mapeia os endpoints dos controllers (ex: /api/auth)
app.MapControllers();

// Mapeia o Hub do SignalR para o endpoint "/chathub"
app.MapHub<ChatHub>("/chathub");

// Inicia a aplica��o
app.Run();