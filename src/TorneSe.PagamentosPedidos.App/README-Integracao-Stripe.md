# Integração com Stripe - TorneSe.PagamentosPedidos.App

Este documento descreve a implementação da integração com o gateway de pagamentos Stripe no projeto AWS Lambda, incluindo a integração com DynamoDB para buscar dados dos pedidos.

## Estrutura da Implementação

### 1. Abstrações (Interfaces)
- **`IPagamentoService`**: Interface principal para operações de pagamento
- **`IDbService`**: Interface para operações de banco de dados
- **`CriarPagamentoDto`**: DTO para criação de pagamentos
- **`PagamentoStatusDto`**: DTO para status de pagamentos

### 2. Implementação (Infraestrutura)
- **`StripePagamentoService`**: Implementação concreta do serviço de pagamento
- **`StripeWebhookService`**: Serviço para processar webhooks do Stripe
- **`DbService`**: Implementação para consultas no DynamoDB
- **`PedidoMappingProfile`**: Perfil do AutoMapper para mapeamento de dados

### 3. Modelos de Dados
- **`PedidoDynamoModel`**: Modelo para dados do DynamoDB
- **`PedidoCompletoModel`**: Modelo para estrutura JSON do pedido
- **`ItemPedidoModel`**: Modelo para itens do pedido
- **`FormaPagamentoModel`**: Modelo para formas de pagamento

### 4. Use Cases
- **`IniciarPagamento`**: Criar um novo pagamento (busca dados no DynamoDB)
- **`ConsultarStatusPagamento`**: Consultar status de um pagamento
- **`CancelarPagamento`**: Cancelar um pagamento

## Configuração

### 1. Configuração do Stripe
Adicione as seguintes configurações no `appsettings.json`:

```json
{
  "Stripe": {
    "SecretKey": "sk_test_your_stripe_secret_key_here",
    "PublishableKey": "pk_test_your_stripe_publishable_key_here",
    "WebhookSecret": "whsec_your_webhook_secret_here"
  }
}
```

### 2. Configuração do DynamoDB
A tabela `Pedidos` deve ter a seguinte estrutura:
- **HashKey**: `DataPedido` (string)
- **RangeKey**: `Id` (string)
- **Campos**: `PedidoCompleto` (JSON string), `ValorTotal` (decimal), `Status` (string)

### 3. Variáveis de Ambiente (AWS Lambda)
Configure as seguintes variáveis de ambiente:
- `Stripe__SecretKey`: Chave secreta do Stripe
- `Stripe__WebhookSecret`: Secret do webhook do Stripe
- `AWS_REGION`: Região do AWS (ex: us-east-1)

## Uso dos Use Cases

### 1. Criar Pagamento (Atualizado)

```csharp
var request = new CriarPagamentoRequest
{
    IdPedido = "PED-001"
};

var resultado = await mediator.Send(request);
```

**Fluxo do processo:**
1. Recebe o `IdPedido` na request
2. Busca os dados completos do pedido no DynamoDB
3. Deserializa o JSON `PedidoCompleto` para obter dados do cliente
4. Mapeia os dados usando AutoMapper para `CriarPagamentoDto`
5. Cria o pagamento no Stripe
6. Retorna as informações do pagamento

### 2. Consultar Status do Pagamento

```csharp
var request = new ConsultarStatusPagamentoRequest
{
    PaymentIntentId = "pi_1234567890"
};

var resultado = await mediator.Send(request);
```

### 3. Cancelar Pagamento

```csharp
var request = new CancelarPagamentoRequest
{
    PaymentIntentId = "pi_1234567890",
    MotivoCancelamento = "Solicitação do cliente"
};

var resultado = await mediator.Send(request);
```

## Estrutura do Pedido no DynamoDB

### Exemplo de JSON do campo `PedidoCompleto`:

```json
{
  "nome": "Maria Silva",
  "email": "maria.silva@email.com",
  "telefone": "(11) 98765-4321",
  "logradouro": "Avenida Paulista",
  "numero": "1578",
  "complemento": "Apto 42, Bloco B",
  "bairro": "Bela Vista",
  "cidade": "São Paulo",
  "estado": "SP",
  "cep": "01310-200",
  "itens": [
    {
      "nomeProduto": "Smartphone Galaxy S23",
      "valor": 3499.90,
      "quantidade": 1,
      "idSku": 1089745
    }
  ],
  "formasPagamento": [
    {
      "tipo": "cartao_credito",
      "valor": 2500.00,
      "parcelas": 3,
      "tokenCartao": "eyJhbGciOiJIUzI1NiJ9...",
      "bandeira": "mastercard"
    }
  ]
}
```

## Mapeamento Automático

O AutoMapper mapeia automaticamente os seguintes campos:

- **IdPedido**: `PedidoDynamoModel.Id`
- **Valor**: `PedidoDynamoModel.ValorTotal`
- **Moeda**: "brl" (fixo)
- **Descricao**: "Pagamento do pedido {Id}"
- **EmailCliente**: Extraído do JSON `PedidoCompleto`
- **NomeCliente**: Extraído do JSON `PedidoCompleto`
- **Metadados**: Inclui informações do pedido e cliente

## Webhooks

O serviço `StripeWebhookService` processa os seguintes eventos do Stripe:

- **`payment_intent.succeeded`**: Pagamento confirmado
- **`payment_intent.payment_failed`**: Pagamento falhou
- **`payment_intent.canceled`**: Pagamento cancelado

### Configuração de Webhook no Stripe

1. Acesse o dashboard do Stripe
2. Vá para Developers > Webhooks
3. Adicione um novo endpoint com a URL do seu Lambda
4. Selecione os eventos:
   - `payment_intent.succeeded`
   - `payment_intent.payment_failed`
   - `payment_intent.canceled`
5. Copie o webhook secret e configure no `appsettings.json`

## Tratamento de Erros

A implementação inclui tratamento robusto de erros:

- **StripeException**: Erros específicos da API do Stripe
- **Exception**: Erros gerais do sistema
- **Pedido não encontrado**: Retorna erro específico
- **Logging**: Logs detalhados para debugging

## Logs

O sistema gera logs para todas as operações:

- Consulta de pedidos no DynamoDB
- Criação de pagamentos
- Consulta de status
- Cancelamento de pagamentos
- Processamento de webhooks
- Erros e exceções

## Segurança

- Chaves do Stripe são configuradas via variáveis de ambiente
- Validação de assinatura de webhooks
- Tratamento seguro de dados sensíveis
- Acesso ao DynamoDB via IAM roles

## Testes

Para testar a integração:

1. Use chaves de teste do Stripe
2. Configure webhooks para ambiente de desenvolvimento
3. Crie registros de teste na tabela DynamoDB
4. Teste todos os Use Cases
5. Verifique os logs para confirmar o funcionamento

## Dependências

- **Stripe.net**: Versão 48.4.0
- **AWSSDK.DynamoDBv2**: Para acesso ao DynamoDB
- **MediatR**: Para implementação dos Use Cases
- **AutoMapper**: Para mapeamento de dados
- **Microsoft.Extensions.Logging**: Para logging
- **Microsoft.Extensions.Configuration**: Para configurações

## Próximos Passos

1. Implementar testes unitários
2. Adicionar validações de entrada
3. Implementar retry policy para falhas de rede
4. Adicionar métricas e monitoramento
5. Implementar cache para consultas frequentes
6. Otimizar consultas do DynamoDB (usar GSI em vez de Scan)
