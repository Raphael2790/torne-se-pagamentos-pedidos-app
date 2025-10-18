# GitHub Copilot — Instruções do Repositório (.NET 8+)

Estas instruções são para uso com o GitHub Copilot, orientando a geração de código em C# 12 e .NET 8+ (ASP.NET Core, EF Core 8, Worker Services). Elas cobrem padrões de código, segurança, desempenho, testes e práticas recomendadas para APIs e serviços backend.

## 1) Contexto e Objetivo
- Linguagem: **C# 12**, Framework: **.NET 8+** (ASP.NET Core, EF Core 8, Worker Services).  
- Plataformas de destino: Linux containers (Alpine/Debian) e Windows para desenvolvimento local.  
- Objetivo: **código seguro, testável, performático e observável**, priorizando clareza e manutenção.

---

## 2) Estilo, Idiomas e Formato de Resposta do Copilot
- Responda em **português do Brasil**.
- Para código: fornecer **arquivos completos** (incluindo `using`, namespace e `Program.cs` quando pertinente).
- Inclua **comentários curtos** explicando decisões-chave.
- Prefira **exemplos mínimos funcionais (MVP)** e **passos objetivos** (comandos `dotnet` e anotações quando necessário).

---

## 3) Padrões de Código (C# 12 / .NET 8)
- **Nullable enable** em todos os projetos; trate warnings e **`-warnaserror`**.
- **Async-first**: use `async/await`, `CancellationToken` obrigatório em I/O e endpoints.  
  Evite *sync-over-async*. Use `ValueTask` **apenas** em hot paths comprovados.
- **Usings implícitos** e **namespaces file-scoped**.  
- **Records/readonly struct** para DTOs imutáveis e tipos de valor sem boxing.  
- **Collection expressions** e pattern matching modernos quando melhorarem legibilidade.  
- Não usar **`dynamic`**, Reflection e `Activator.CreateInstance` fora de *composition roots*.  
- **Analisadores**: habilite .NET Analyzers + StyleCop (ou EditorConfig equivalente). Trate avisos.

---

## 5) APIs (ASP.NET Core Minimal/API Controllers)
- **Versionamento** (e.g., `v1`, `v2`), **OpenAPI** com descrições claras e exemplos.  
- **Problem Details** padronizado para erros; middleware de exceções centralizado.  
- **Validação**: DataAnnotations ou FluentValidation nos DTOs de entrada.  
- **Paginação/Ordenação/Filtragem**: parâmetros explícitos; limites de página seguros.  
- **Rate Limiting** (ASP.NET Core RateLimiter) quando exposto publicamente.  
- **Idempotência** para operações sensíveis (chaves idempotentes, *outbox* para side-effects).

---

## 6) Segurança
- **HTTPS only**, **HSTS** e cabeçalhos seguros.  
- **Autenticação**: preferir **JWT Bearer**; **Autorização por políticas** (roles/claims).  
- **Entrada**: sanitização e validação rígida. Nunca concatene SQL/paths.  
- **Segredos**: nunca em repositório; usar Secret Manager no dev e variáveis/KeyVault em ambientes.  
- **Serialização**: `System.Text.Json` com naming policy consistente; **não** usar *binary formatter*.  
- **Uploads**: tamanhos limitados, validação de conteúdo e *streaming* quando aplicável.

---

## 7) Observabilidade e Resiliência
- **Logging estruturado** via `Microsoft.Extensions.Logging` (Serilog opcional); inclua `CorrelationId/TraceId`.  
- **OpenTelemetry** (traces/metrics/logs) integrado à API/Jobs.  
- **Health Checks** para dependências críticas.  
- **Resiliência**: `HttpClientFactory` com *policies* (retry com *jitter*, timeout, circuit breaker) usando Polly.  
- **Métricas-chave**: latência por endpoint, taxa de erro, throughput, uso de memória/GC de jobs.

---

## 8) Dados e EF Core 8
- **DbContext pooling**; **`AsNoTracking`** para leitura; nunca *Lazy Loading*.  
- **Consultas**: evite N+1; use projeções e **consultas compiladas** em hot paths.  
- **Transações**: delimitar escopo; **Outbox** para integração/entrega confiável.  
- **Migrações** versionadas; **Seed** apenas para desenvolvimento.  
- **Mapeamentos**: enums como string (quando estabilidade de nome), comprimentos e índices explícitos.  
- **Concurrency tokens** em entidades críticas.

---

## 9) Performance e Escalabilidade
- **Síncrono vs assíncrono**: prefira *async*.  
- **Caching**: `IMemoryCache`/Response Caching; invalidação explícita.  
- **Minimizar alocações** (evite *boxing*, LINQ em hot paths, use `Span<T>`/ArrayPool com parcimônia).  
- **Config Kestrel**: limites de request, *keep-alive*, *max concurrent streams* conforme cenário.  
- **Background jobs**: `IHostedService`/Worker + filas; *backpressure* e *bounded concurrency*.

---

## 10) Configuração
- `appsettings.json` + `appsettings.{Environment}.json` + **Environment Variables**.  
- **Options pattern** (`IOptions<T>`) com **validação** (`ValidateOnStart`).  
- Um único **ponto de binding** para cada options type; não *bind* repetido.

---

## 11) Testes
- **xUnit** + **FluentAssertions** (AAA).  
- **Testes de unidade** para domínio/serviços; **integração** com `WebApplicationFactory` e **Testcontainers** para DB/filas.  
- **Cobertura crítica**: *domain logic*, validações, endpoints públicos.  
- *Mocks* apenas nas bordas; prefira *in-memory* ou containers para integrações.  
- **BenchmarkDotNet** para hot paths quando necessário.

---

## 12) CI/CD (orientações ao Copilot)
- Gerar *pipelines* com: `dotnet restore/build/test`, `dotnet format --verify-no-changes`, `dotnet test /p:CollectCoverage=true`, **analisadores** e **SCA**.  
- Publicação com `dotnet publish -c Release -r linux-x64 --self-contained false`.  
- Imagens Docker slim, *multi-stage*, *non-root user*.

---

## 13) Convenções Git e PRs
- **Conventional Commits** (`feat:`, `fix:`, `chore:`, `refactor:`…).  
- PRs pequenos e focados; inclua **descrição, screenshots** (se UI), **checagens de segurança**, e **itens testados**.

---

## 14) O que **não** fazer
- Não expor detalhes internos em mensagens de erro.  
- Não ignorar cancellation tokens.  
- Não suprimir warnings sem justificativa.  
- Não retornar entidades EF diretamente pelos endpoints; use DTOs.  
- Não capturar exceções genéricas silenciosamente.

---

## 15) Prompts de Referência (para você, Copilot)
- “Crie um endpoint Minimal API `POST /v1/orders` com validação FluentValidation, ProblemDetails para erros e testes xUnit usando WebApplicationFactory.”  
- “Implemente repositório EF Core 8 com consultas compiladas para leitura e transação para gravação; inclua teste de integração usando Testcontainers (SQL Server).”  
- “Adicione OpenTelemetry (traces + logs) e HealthChecks (DB e fila) com endpoints `/health` e `/health/ready`.”  
- “Escreva middleware de exceções que converte exceções de domínio em ProblemDetails, com correlação de `TraceId`.”  
- “Gere pipeline GitHub Actions com dotnet format, analyzers e testes com cobertura; falhe em warnings.”

---

## 16) Exigências ao gerar código
- Incluir **tratamento de erros**, **validação**, **cancellation token**, **logs úteis** (níveis corretos).  
- Produzir **exemplos executáveis** (incluindo `Program.cs` e DI) quando aplicável.  
- Aderir às seções 3–12 deste documento.  
- Se existir regra conflitante aqui, **explique a decisão** no comentário do código.

---

### Apêndice A — Pacotes NuGet Sugeridos
- **FluentValidation**, **Serilog** (+ sinks), **Polly**, **OpenTelemetry** (API/Exporter/Extensions), **Testcontainers**, **BenchmarkDotNet**, **Swashbuckle.AspNetCore**.  
- Adicione apenas quando necessário e justificado.

---

### Apêndice B — Checklists Rápidos
- **Endpoint**: validação ✓ | ProblemDetails ✓ | Authz ✓ | CT ✓ | logs ✓ | testes ✓  
- **EF Core**: `AsNoTracking` leitura ✓ | transação ✓ | migração ✓ | índice ✓  
- **Job/Worker**: backoff/jitter ✓ | limites de concorrência ✓ | CT e *graceful shutdown* ✓
- **Configuração**: Options pattern ✓ | validação ✓ | ambiente ✓  
- **Segurança**: HTTPS ✓ | sanitização ✓ | segredos ✓ | headers ✓
- **Observabilidade**: logs estruturados ✓ | OpenTelemetry ✓ | HealthChecks ✓
- **Performance**: caching ✓ | minimizar alocações ✓ | Kestrel config ✓
- **Testes**: unitários ✓ | integração ✓ | cobertura ✓ | benchmarks ✓

---

Estas instruções devem ser revisadas periodicamente para refletir as melhores práticas e atualizações do .NET.

*Fim das Instruções*