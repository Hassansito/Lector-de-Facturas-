using Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Repositories.Interfaces;
using Repositories.Repositories;
using Services.Interfaces;
using Services.Mappings;
using Services.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 52428800; // 50 MB
});

//  Aumentar el límite para el manejo de formularios y JSON
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 52428800; // 50 MB
    options.ValueLengthLimit = int.MaxValue;
});
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

// Add services to the container.

builder.Services.AddControllers();
//builder.Services.AddControllers(options =>
//{
//    options.Filters.Add<ApiExceptionFilterAttribute>();
//});

// ===== CORS AGREGADO AQUÍ =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("https://localhost:44304")  // Origen de tu Blazor
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();  
    });
});
// ===== FIN CORS =====

builder.Services.AddSignalR();
builder.Services.AddSingleton<IFileQueue, FileQueue>();
builder.Services.AddScoped<IFacturaParser, EtecsaFacturaParser>();
builder.Services.AddSingleton<IProgressService, ProgressService>();
builder.Services.AddHostedService<FacturaWorker>();
builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddScoped<IClientePDF, ClientesPDFRepository>();
builder.Services.AddScoped<ICLienteService, ClienteService>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Connection")));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Login / Authentication
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
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ITokenService, TokenService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

// ===== CORS AGREGADO AQUÍ (ANTES DE AUTENTICACIÓN) =====
app.UseCors("AllowAll");
// ===== FIN CORS =====

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<FacturaProgressHub>("/facturaProgress");

app.Run();