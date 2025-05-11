---

# 🚀 **InsightHub**  
🔎 **Monitoramento Inteligente de APIs e Serviços**

Uma plataforma avançada de monitoramento que combina **.NET Aspire**, **Prometheus** e **Grafana** para rastrear **métricas de desempenho, erros e tempo de resposta** de APIs e sistemas em tempo real.

## 📊 **Principais Recursos**
✅ **Monitoramento de APIs via Prometheus**  
✅ **Dashboards avançados no Grafana**  
✅ **Análise de tempo de resposta e erros**  
✅ **Integração com Redis e SQL Server**  
✅ **Métricas de uso de CPU e memória**  
✅ **Alertas automáticos para erros e alto consumo de recursos**  

---

## 🔧 **Tecnologias Utilizadas**
🔹 **.NET Aspire** – Orquestração e serviços  
🔹 **Prometheus** – Coleta de métricas  
🔹 **Grafana** – Dashboards de monitoramento  
🔹 **Redis** – Cache e armazenamento de usuários  
🔹 **SQL Server** – Banco de dados para produtos  
🔹 **Docker** – Containers para serviços  

---

## 📦 **Estrutura do Projeto**
```sh
InsightHub/
│── MeuProjetoAspire.sln      # Solução principal do projeto
│── MeuProjetoAspire.ApiService/   # Backend das APIs
│── MeuProjetoAspire.AppHost/      # Orquestração dos serviços
│── MeuProjetoAspire.ServiceDefaults/  # Configurações globais
│── MeuProjetoAspire.Web/          # Frontend Blazor
│── grafana-data/                  # Dados do Grafana
│── prometheus-data/                # Dados do Prometheus
│── prometheus.yml                   # Configuração do Prometheus
```

---

## 🚀 **Instalação e Execução**
### 🔹 **1. Clone o Repositório**
```sh
git clone https://github.com/renatotgakishi/InsightHub.git
cd InsightHub
```

### 🔹 **2. Configurar o Ambiente**
Certifique-se de ter **Docker** instalado para rodar os serviços. Para configurar os containers, execute:
```sh
docker-compose up -d
```

### 🔹 **3. Executar o Backend (.NET Aspire)**
```sh
cd MeuProjetoAspire.ApiService
dotnet run
```

### 🔹 **4. Acessar o Frontend (Blazor)**
```sh
cd MeuProjetoAspire.Web
dotnet run
```
Abra no navegador: `http://localhost:5000`

### 🔹 **5. Acessar o Grafana para Dashboards**
Após iniciar os containers, acesse:
🔗 `http://localhost:3000`  
🔗 Login padrão: `admin/admin`

### 🔹 **6. Verificar métricas no Prometheus**
🔗 `http://localhost:9090`

---

## 📊 **Dashboards no Grafana**
Aqui estão alguns gráficos que podem ser configurados no **Grafana** para monitoramento:

✅ **Tempo médio de resposta por endpoint**  
```promql
histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m]))
```

✅ **Taxa de erro por API (códigos 4xx, 5xx)**  
```promql
sum(rate(http_requests_total{status=~"4..|5.."}[5m])) by (status)
```

✅ **Quantidade de requisições por minuto**  
```promql
sum(rate(http_requests_total[1m])) by (method)
```

✅ **Uso de CPU e memória dos serviços**  
```promql
rate(process_cpu_seconds_total[1m])
process_resident_memory_bytes
```

---

## 🤝 **Contribuições**
Sinta-se à vontade para contribuir!  
📌 **Fork o projeto**  
📌 **Crie um branch** (`git checkout -b feature-nova`)  
📌 **Commit suas mudanças** (`git commit -m "Nova funcionalidade"`)  
📌 **Faça um push** (`git push origin feature-nova`)  
📌 **Abra um Pull Request**  

---

## 📄 **Licença**
Este projeto está licenciado sob a **MIT License** – veja o arquivo [`LICENSE`](LICENSE) para mais detalhes.

---
