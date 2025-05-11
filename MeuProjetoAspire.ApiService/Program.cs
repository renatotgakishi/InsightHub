using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using Microsoft.AspNetCore.Mvc;

using OpenTelemetry.Metrics; 
using OpenTelemetry.Trace;  

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations
builder.AddServiceDefaults();

// Add services to the container
builder.Services.AddProblemDetails();

// Configuração do Swagger
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API de Produtos",
        Version = "v1",
        Description = "API para gerenciamento de produtos"
    });
});

// Configuração CORRETA do SQL Server DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MeuBancoDeDados")));


// Configuração segura do Redis com verificação de nulo
var redisConnectionString = builder.Configuration.GetConnectionString("redis-cache") 
    ?? throw new InvalidOperationException("Connection string 'redis-cache' não encontrada.");

// Registro explícito do IConnectionMultiplexer como singleton
builder.Services.AddSingleton<IConnectionMultiplexer>(_ => 
    ConnectionMultiplexer.Connect(redisConnectionString));

    
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "SampleInstance_";
});

// Health Check com verificação de nulo
builder.Services.AddHealthChecks()
    .AddRedis(
        redisConnectionString: redisConnectionString,
        name: "redis",
        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
        tags: new[] { "cache", "redis" });

var app = builder.Build();


// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => 
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API de Produtos v1");
    });
}

app.UseExceptionHandler();

// Configuração do banco de dados
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}

// Endpoints de Produtos
app.MapGet("/produtos", async (AppDbContext db) => 
    await db.Produtos.ToListAsync());

app.MapGet("/produtos/{id}", async (int id, AppDbContext db) =>
    await db.Produtos.FindAsync(id) is Produto produto
        ? Results.Ok(produto)
        : Results.NotFound());

app.MapPost("/produtos", async (Produto produto, AppDbContext db) =>
{
    db.Produtos.Add(produto);
    await db.SaveChangesAsync();
    return Results.Created($"/produtos/{produto.Id}", produto);
}).WithOpenApi();

app.MapPut("/produtos/{id}", async (int id, Produto inputProduto, AppDbContext db) =>
{
    var produto = await db.Produtos.FindAsync(id);
    
    if (produto is null) return Results.NotFound();
    
    produto.Nome = inputProduto.Nome;
    produto.Preco = inputProduto.Preco;
    
    await db.SaveChangesAsync();
    return Results.NoContent();
}).WithOpenApi();

app.MapDelete("/produtos/{id}", async (int id, AppDbContext db) =>
{
    if (await db.Produtos.FindAsync(id) is Produto produto)
    {
        db.Produtos.Remove(produto);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    return Results.NotFound();
}).WithOpenApi();


// Endpoints de Usuários (Redis) - Versão Corrigida
app.MapGet("/usuarios/{id}", async (
    [FromRoute] string id, 
    [FromServices] IConnectionMultiplexer redis) =>
{
    try
    {
        var db = redis.GetDatabase();
        var usuarioJson = await db.StringGetAsync($"usuario:{id}");
        
        if (!usuarioJson.HasValue)
            return Results.NotFound();
        
        var usuario = JsonSerializer.Deserialize<Usuario>(usuarioJson!);
        return Results.Ok(usuario);
    }
    catch (RedisException ex)
    {
        return Results.Problem($"Erro no Redis: {ex.Message}");
    }
}).WithOpenApi();

app.MapPost("/usuarios", async (
    [FromBody] Usuario usuario, 
    [FromServices] IConnectionMultiplexer redis) =>
{
    try
    {
        var db = redis.GetDatabase();
        var serialized = JsonSerializer.Serialize(usuario);
        var success = await db.StringSetAsync($"usuario:{usuario.Id}", serialized);
        
        return success 
            ? Results.Created($"/usuarios/{usuario.Id}", usuario)
            : Results.Problem("Falha ao criar usuário");
    }
    catch (RedisException ex)
    {
        return Results.Problem($"Erro no Redis: {ex.Message}");
    }
}).WithOpenApi();

app.MapGet("/usuarios", async (
    [FromServices] IConnectionMultiplexer redis) =>
{
    try
    {
        var db = redis.GetDatabase();
        var endpoints = redis.GetEndPoints();
        if (endpoints.Length == 0)
            return Results.Problem("Nenhum endpoint Redis disponível");

        var server = redis.GetServer(endpoints[0]);
        var keys = server.Keys(pattern: "usuario:*");
        
        var usuarios = new List<Usuario>();
        foreach (var key in keys)
        {
            var usuarioJson = await db.StringGetAsync(key);
            if (usuarioJson.HasValue)
            {
                var usuario = JsonSerializer.Deserialize<Usuario>(usuarioJson!);
                if (usuario != null)
                    usuarios.Add(usuario);
            }
        }
        
        return Results.Ok(usuarios);
    }
    catch (RedisException ex)
    {
        return Results.Problem($"Erro no Redis: {ex.Message}");
    }
}).WithOpenApi();

app.MapDefaultEndpoints();

app.Run();

// Modelo inline (alternativa se não quiser criar arquivo separado)
public record Usuario(string Id, string Nome, string Email);
public class Produto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    
    [Column(TypeName = "decimal(18,2)")]    
    public decimal Preco { get; set; }
}

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Produto> Produtos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Produto>().HasData(
            new Produto { Id = 1, Nome = "Produto 1", Preco = 10.99m },
            new Produto { Id = 2, Nome = "Produto 2", Preco = 20.50m }
        );
    }
}