using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

//--------------------------------------------------------------------------------
// Configuração do Grafana corrigida
var grafana = builder.AddContainer("grafana", "grafana/grafana")
    .WithEndpoint(
        name: "grafana-http",
        targetPort: 3000,  // Porta dentro do container
        scheme: "http")    // Tipo de protocolo
    .WithEnvironment("GF_SECURITY_ADMIN_PASSWORD", "admin123")
    .WithVolume("grafana-data", "/var/lib/grafana");

// Configuração do Prometheus corrigida
var prometheus = builder.AddContainer("prometheus", "prom/prometheus")
    .WithEndpoint(
        name: "prometheus-http",
        targetPort: 9090,
        scheme: "http")
    .WithVolume("prometheus-data", "/prometheus")
    .WithBindMount(
        source: Path.GetFullPath(Path.Combine(builder.AppHostDirectory, "prometheus.yml")),
        target: "/etc/prometheus/prometheus.yml")
    .WithEnvironment("PROMETHEUS_CONFIG_FILE", "/etc/prometheus/prometheus.yml");
//--------------------------------------------------------------------------------

// Configuração do Redis
var redis = builder.AddRedis("redis-cache")
    .WithRedisInsight();

// Configuração do SQL Server
var sqlServer = builder.AddSqlServer("sqlserver")
    .AddDatabase("MeuBancoDeDados");

// Configuração do API Service
var apiService = builder.AddProject<Projects.MeuProjetoAspire_ApiService>("apiservice")
    .WithReference(redis)
    .WithReference(sqlServer);
   

// Configuração do Web Frontend
builder.AddProject<Projects.MeuProjetoAspire_Web>("webfrontend")
    .WithReference(apiService);

builder.Build().Run();