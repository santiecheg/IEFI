using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Verduleria.Business;
using Verduleria.Data;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<VerduleriaDbContext>(options =>
    options.UseSqlServer(connectionString));

// Servicios de la capa Business
builder.Services.AddScoped<ProductoService>();
builder.Services.AddScoped<CajeroService>();
builder.Services.AddScoped<VentaService>();

// CORS simple para consumir la API desde HTML/JS vainilla
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();