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

## Roteamento de Eventos

A Function Lambda processa mensagens SQS baseadas no atributo `evento` presente no corpo da mensagem. Os eventos suportados são:

- **`iniciar pagamento`**: Cria um novo pagamento no Stripe
- **`consultar pagamento`**: Consulta o status de um pagamento
- **`cancelar pagamento`**: Cancela um pagamento existente

## Exemplos de Mensagens SQS

### 1. Iniciar Pagamento

```json
{
  "Records": [
    {
      "messageId": "19dd0b57-b21e-4ac1-bd88-01bbb068cb78",
      "receiptHandle": "MessageReceiptHandle",
      "body": "{\"evento\": \"iniciar pagamento\", \"idPedido\": \"PED-001\", \"status\": \"pendente\", \"dataPedido\": \"2024-01-15\", \"dataHoraEvento\": \"2024-01-15T10:30:00Z\"}",
      "attributes": {
        "ApproximateReceiveCount": "1",
        "SentTimestamp": "1523232000000",
        "SenderId": "123456789012",
        "ApproximateFirstReceiveTimestamp": "1523232000001"
      },
      "messageAttributes": {},
      "md5OfBody": "7b270e59b47ff90a553787216d55d91d",
      "eventSource": "aws:sqs",
      "eventSourceARN": "arn:aws:sqs:us-east-1:123456789012:pagamentos-pedidos-queue",
      "awsRegion": "us-east-1"
    }
  ]
}
```

**Fluxo do processo:**
1. Recebe o `IdPedido` na mensagem SQS
2. Busca os dados completos do pedido no DynamoDB usando `DataPedido` e `IdPedido`
3. Deserializa o JSON `PedidoCompleto` para obter dados do cliente
4. Mapeia os dados usando AutoMapper para `CriarPagamentoDto`
5. Cria o pagamento no Stripe
6. Retorna as informações do pagamento

### 2. Consultar Status do Pagamento

```json
{
  "Records": [
    {
      "messageId": "19dd0b57-b21e-4ac1-bd88-01bbb068cb79",
      "receiptHandle": "MessageReceiptHandle",
      "body": "{\"evento\": \"consultar pagamento\", \"paymentIntentId\": \"pi_1234567890abcdef\"}",
      "attributes": {
        "ApproximateReceiveCount": "1",
        "SentTimestamp": "1523232000000",
        "SenderId": "123456789012",
        "ApproximateFirstReceiveTimestamp": "1523232000001"
      },
      "messageAttributes": {},
      "md5OfBody": "7b270e59b47ff90a553787216d55d91e",
      "eventSource": "aws:sqs",
      "eventSourceARN": "arn:aws:sqs:us-east-1:123456789012:pagamentos-pedidos-queue",
      "awsRegion": "us-east-1"
    }
  ]
}
```

### 3. Cancelar Pagamento

```json
{
  "Records": [
    {
      "messageId": "19dd0b57-b21e-4ac1-bd88-01bbb068cb80",
      "receiptHandle": "MessageReceiptHandle",
      "body": "{\"evento\": \"cancelar pagamento\", \"paymentIntentId\": \"pi_1234567890abcdef\", \"motivoCancelamento\": \"Cliente solicitou cancelamento\"}",
      "attributes": {
        "ApproximateReceiveCount": "1",
        "SentTimestamp": "1523232000000",
        "SenderId": "123456789012",
        "ApproximateFirstReceiveTimestamp": "1523232000001"
      },
      "messageAttributes": {},
      "md5OfBody": "7b270e59b47ff90a553787216d55d91f",
      "eventSource": "aws:sqs",
      "eventSourceARN": "arn:aws:sqs:us-east-1:123456789012:pagamentos-pedidos-queue",
      "awsRegion": "us-east-1"
    }
  ]
}
```

## Tratamento de Erros

### Evento Não Reconhecido

```json
{
  "Records": [
    {
      "messageId": "19dd0b57-b21e-4ac1-bd88-01bbb068cb81",
      "receiptHandle": "MessageReceiptHandle",
      "body": "{\"evento\": \"evento inexistente\", \"idPedido\": \"PED-001\"}",
      "attributes": {
        "ApproximateReceiveCount": "1",
        "SentTimestamp": "1523232000000",
        "SenderId": "123456789012",
        "ApproximateFirstReceiveTimestamp": "1523232000001"
      },
      "messageAttributes": {},
      "md5OfBody": "7b270e59b47ff90a553787216d55d920",
      "eventSource": "aws:sqs",
      "eventSourceARN": "arn:aws:sqs:us-east-1:123456789012:pagamentos-pedidos-queue",
      "awsRegion": "us-east-1"
    }
  ]
}
```

### Mensagem Sem Atributo Evento

```json
{
  "Records": [
    {
      "messageId": "19dd0b57-b21e-4ac1-bd88-01bbb068cb82",
      "receiptHandle": "MessageReceiptHandle",
      "body": "{\"idPedido\": \"PED-001\", \"status\": \"pendente\"}",
      "attributes": {
        "ApproximateReceiveCount": "1",
        "SentTimestamp": "1523232000000",
        "SenderId": "123456789012",
        "ApproximateFirstReceiveTimestamp": "1523232000001"
      },
      "messageAttributes": {},
      "md5OfBody": "7b270e59b47ff90a553787216d55d921",
      "eventSource": "aws:sqs",
      "eventSourceARN": "arn:aws:sqs:us-east-1:123456789012:pagamentos-pedidos-queue",
      "awsRegion": "us-east-1"
    }
  ]
}
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

## Logs Otimizados

O sistema implementa logs otimizados seguindo boas práticas:

### Logs de Entrada (Críticos)
- **Entrada de requisições**: Log de entrada com dados principais para identificação
- **Eventos identificados**: Log do tipo de evento processado

### Logs de Erro (Críticos)
- **Exceções**: Todos os erros são logados com stack trace completo
- **Falhas de negócio**: Erros específicos do Stripe e DynamoDB
- **Pedidos não encontrados**: Warnings para pedidos inexistentes

### Logs Removidos (Não Críticos)
- Logs de sucesso intermediários
- Logs de progresso de operações
- Logs de debug desnecessários

### Exemplo de Logs Gerados

```
[INFO] Processando mensagem SQS - MessageId: 19dd0b57-b21e-4ac1-bd88-01bbb068cb78
[INFO] Evento identificado: iniciar pagamento
[INFO] Processando pagamento - IdPedido: PED-001, DataPedido: 2024-01-15
[ERROR] Pedido não encontrado no DynamoDB: DataPedido=2024-01-15, IdPedido=PED-001
[ERROR] Falha ao iniciar pagamento - IdPedido: PED-001, Erro: Pedido não encontrado: PED-001
```

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
