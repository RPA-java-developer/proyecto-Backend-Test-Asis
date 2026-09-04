using ASISYA.Data;
using ASISYA.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;





var builder = WebApplication.CreateBuilder(args);


builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5146); // Escucha en el puerto 5146 para cualquier IP
});

// Add services to the container to support controllers and views.
//builder.Services.AddControllersWithViews();


// Cadena de conexión desde appsettings.json
var connectionString = builder.Configuration.GetConnectionString("cadenaMySQL")
    ?? throw new InvalidOperationException("La cadena de conexión 'cadenaMySQL' no se configuró.");



// Registro del DbContext usando MySQL (Pomelo)
/*
builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
*/

// Factory de DbContext: necesaria para crear contexts propios dentro
// del BackgroundService, fuera del ciclo de vida de un request HTTP.
builder.Services.AddDbContextFactory<AppDBContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));


// Servicios para controladores (API)
builder.Services.AddControllers();


builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;

});


// --- Carga masiva de productos ---
builder.Services.AddSingleton<IBulkImportJobStore, InMemoryBulkImportJobStore>();
builder.Services.AddSingleton<IBulkImportQueue, BulkImportQueue>();
builder.Services.AddScoped<IBulkProductImportService, BulkProductImportService>();
builder.Services.AddHostedService<BulkImportBackgroundWorker>();



// --- Autenticación JWT ---
builder.Services.AddScoped<IJwtService, JwtService>();

var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSettings["Key"]!;


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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});


builder.Services.AddAuthorization();


// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.ParameterLocation.Header,
        Description = "Pega solo el token (sin la palabra 'Bearer')."
    });

    options.AddSecurityRequirement(document => new Microsoft.OpenApi.OpenApiSecurityRequirement
    {
        [new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});

// CORS (opcional, útil si vas a consumir la API desde un frontend separado)
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


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}


// --- Activación del middleware de Swagger (DESPUÉS de Build) ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


//app.UseHttpsRedirection();
//app.UseStaticFiles();


app.UseCors("AllowAll");


// Add Middleware (The order here is strictly critical)
app.UseAuthentication(); // Determines who you are
app.UseAuthorization();  // Determines what you can do

/*
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
*/
app.MapGet("/", () => "¡Hola desde el puerto 5146!");


app.MapControllers();


app.Run();
