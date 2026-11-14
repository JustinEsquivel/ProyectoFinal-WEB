using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ProyectoFinal_API.Middleware;
using ProyectoFinal_API.Services;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuración personal de cada integrante (cadena de conexión, correo).
// Este archivo NO se sube a GitHub (está en .gitignore).
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddScoped<ISeguridadService, SeguridadService>();
builder.Services.AddScoped<IBitacoraService, BitacoraService>();

// Seguridad por medio de JWT: la WEB envía el token en el encabezado Authorization: Bearer
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration.GetValue<string>("Jwt:Emisor"),
            ValidAudience = builder.Configuration.GetValue<string>("Jwt:Audiencia"),
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("Jwt:Clave")!)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Administración de excepciones: captura cualquier error, lo guarda en BitacoraError y responde 500
app.UseMiddleware<ManejoErroresMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
