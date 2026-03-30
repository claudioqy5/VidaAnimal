using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using VidaAnimal.API.Data;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// .NET 9 Native OpenAPI
builder.Services.AddOpenApi();

// Configurando la cadena de conexión
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Habilitar politicas CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp", policy =>
    {
        policy.SetIsOriginAllowed(origin => true) // Permitir cualquier origen en producción para evitar bloqueos de IP
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ============================================
// CONFIGURACIÓN DE JWT (AUTENTICACIÓN Y ROLES)
// ============================================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // En producción, se recomienda usar una variable de entorno "JWT_KEY"
        var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? builder.Configuration["Jwt:Key"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!))
        };
    });

builder.Services.AddAuthorization(); // Activa los roles en los endpoints

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => {
        options.WithTitle("Vida Animal API")
               .WithTheme(ScalarTheme.Mars);
    });
}

app.UseCors("AllowVueApp");

app.UseHttpsRedirection();
app.UseStaticFiles();

// ESTOS DEBEN IR EN ESTE ORDEN (PRIMERO AUTENTICAR, LUEGO AUTORIZAR)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
