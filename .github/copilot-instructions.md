# Copilot Instructions - TorneSe.PagamentosPedidos.App

## Arquitetura do Projeto

Esta é uma **AWS Lambda Function** serverless em **.NET 8** que processa pagamentos via **Stripe**, consumindo mensagens de uma **fila SQS** e persistindo dados em **DynamoDB**. O projeto utiliza **CQRS** simplificado com **MediatR** para organizar os casos de uso.

### Componentes Principais

- **Function.cs**: Entry point da Lambda. Processa mensagens SQS, extrai o atributo `evento` do `MessageAttributes`, e roteia para handlers MediatR
- **Use Cases**: Organizados em `IniciarPagamento`, `ConsultarStatusPagamento`, `CancelarPagamento`, e `ProcessarCancelamentoPagamento` - cada um com `Handler.cs`, `Request/`, e `Response/`
- **Repository Pattern**: `IPedidoRepository` e `IPagamentoRepository` abstraem acesso ao DynamoDB
- **Service Layer**: `StripePagamentoService` gerencia integração com gateway de pagamento Stripe
- **Message Service**: `IMessageService` gerencia envio de mensagens para filas SQS

### Fluxo de Dados

1. **SQS → Lambda**: Mensagem chega com `MessageAttributes["evento"]` determinando a ação
2. **Handler MediatR**: Processa lógica de negócio específica do caso de uso
3. **DynamoDB**: `PedidoRepository` busca dados do pedido usando chaves compostas (`DataPedido` + `Id`)
4. **Stripe API**: `StripePagamentoService` cria/consulta/cancela PaymentIntents
5. **Persistência**: Resultados salvos em tabela `Pagamentos` do DynamoDB
6. **Detecção de Cancelamento**: Ao consultar status, se detectado `canceled` ou `expired`, dispara `ProcessarCancelamentoPagamento` via MediatR
7. **Notificação**: Atualiza status no DynamoDB e envia mensagem para fila `torne-se-fila-pagamento-cancelado`

## DynamoDB - Modelagem de Dados

**Tabela Pedidos**:
- HashKey: `DataPedido` (string formato "yyyy-MM-dd")
- RangeKey: `Id` (ID do pedido)
- Campo `PedidoCompleto`: JSON serializado com dados completos do pedido

**Tabela Pagamentos**:
- HashKey: `IdPedido`
- RangeKey: `PaymentIntentId` (ID retornado pelo Stripe)

⚠️ **IMPORTANTE**: Use `DynamoDBContext` com os modelos decorados com `[DynamoDBTable]`, `[DynamoDBHashKey]`, e `[DynamoDBRangeKey]`. Veja exemplos em `PedidoDynamoModel.cs` e `PagamentoDynamoModel.cs`.

## Padrões do Projeto

### Result Pattern
Todos os handlers retornam `Result<T>` definido em `Common/Result.cs`:
```csharp
public class Result<T> {
    public bool IsSuccess { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }
}
```

### MediatR Requests
Cada Request implementa `IRequest<Result<TResponse>>`:
```csharp
public sealed class Handler : IRequestHandler<CriarPagamentoRequest, Result<CriarPagamentoResponse>>
```

### Primary Constructors
Use **primary constructors** do C# 12 para injeção de dependência nos handlers:
```csharp
public sealed class Handler(ILogger<Handler> logger, IPedidoRepository repository) : IRequestHandler<...>
```

### AutoMapper
Configure mappings em `Mappings/AutoMapperProfile.cs`. Exemplo atual: `PedidoDynamoModel → CriarPagamentoDto`.

## Roteamento de Eventos SQS

A mensagem SQS **DEVE** ter o atributo `evento` em `MessageAttributes` (não no body):
```json
{
  "MessageAttributes": {
    "evento": {
      "StringValue": "iniciar_pagamento",
      "DataType": "String"
    }
  },
  "Body": "{\"idPedido\":\"PED-001\",\"dataPedido\":\"2024-01-15\",...}"
}
```

Eventos suportados:
- `iniciar_pagamento` → `IniciarPagamento/Handler.cs`
- `consultar_pagamento` → `ConsultarStatusPagamento/Handler.cs`
- `cancelar_pagamento` → `CancelarPagamento/Handler.cs`

### Fluxo de Cancelamento Automático

Quando `ConsultarStatusPagamento` detecta status `canceled` ou `expired` do Stripe:
1. Busca informações do pagamento no DynamoDB para obter IdPedido
2. Dispara `CancelarPagamento` via MediatR (reutilização do caso de uso existente)
3. `CancelarPagamento` atualiza status para "Cancelado" na tabela DynamoDB `Pagamentos`
4. Cria `PagamentoCanceladoMessage` com informações do pedido
5. Envia mensagem para fila SQS configurada em `AWS:FilaPagamentoCanceladoUrl`
6. Logs detalhados em todas as etapas para rastreamento

## Integração Stripe

- **API Key**: Configurada via `appsettings.json` → `Stripe:SecretKey` (sobrescrita por env vars em produção)
- **Payment Intent**: Stripe usa centavos (multiplicar valor por 100)
- **Capture Manual**: PaymentIntents criados com `CaptureMethod = "manual"` para controle do fluxo
- **Métodos aceitos**: `["card", "boleto"]`

Detalhes completos em `README-Integracao-Stripe.md`.

## Build e Deploy

### Build Local
```powershell
.\deploy\build-local.ps1
```

### Deploy Completo (CloudFormation)
```powershell
.\deploy\build-and-deploy.ps1 -Region "us-east-1" -BucketName "seu-bucket"
```

O script:
1. Compila com `dotnet publish -c Release -r linux-x64`
2. Cria ZIP excluindo `.pdb` e `.xml`
3. Faz upload para S3
4. Deploy via CloudFormation template (`deploy/template-curso-serverless.yaml`)

### Infraestrutura AWS
- **SQS Queue**: `torne-se-fila-pedido-criado`
- **DLQ**: `torne-se-fila-pedido-criado-dlq` (após 5 tentativas)
- **Lambda Timeout**: 300 segundos
- **IAM Role**: Acesso a SQS, DynamoDB, CloudWatch Logs

## Dependency Injection

Registre novos serviços em `Extensions/ServiceCollectionExtensions.cs`:
```csharp
services.AddScoped<INovoServico, NovoServico>();
```

DI configurado no construtor de `Function.cs` usando `Microsoft.Extensions.DependencyInjection`.

## Convenções de Código

- **Nullable Disable**: `<Nullable>disable</Nullable>` no `.csproj` - não use nullable reference types
- **Implicit Usings**: Habilitado no projeto
- **Logging**: Use `ILogger<T>` injetado, com structured logging: `_logger.LogInformation("Evento: {Evento}", evento)`
- **Português**: Comentários, logs e mensagens em português (conforme `pessoal.instructions.md`)

## Testes

Projeto de testes em `test/TorneSe.PagamentosPedidos.App.Tests/`:
```bash
cd test/TorneSe.PagamentosPedidos.App.Tests
dotnet test
```

## Debugging Local

Use **AWS Lambda Mock Test Tool** ou configure `launchSettings.json` para simular eventos SQS localmente.

## Commits

- Use mensagens de commit claras e descritivas.
- Siga o padrão `tipo(escopo): descrição` para mensagens de commit.
- Exemplos:
  - `feat(pagamentos): adicionar suporte a novos métodos de pagamento`
  - `fix(erro): corrigir falha ao processar pagamentos`