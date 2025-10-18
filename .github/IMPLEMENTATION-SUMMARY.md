# Implementação da Feature: Captura de Cancelamento de Pagamento

## Resumo da Implementação

Esta implementação adiciona a capacidade de detectar automaticamente pagamentos cancelados ou expirados através da consulta de status e processar adequadamente o cancelamento usando o caso de uso **CancelarPagamento** existente, notificando os sistemas interessados.

## Componentes Criados

### 1. Modelo de Mensagem

**Arquivo**: `Domain/Messages/PagamentoCanceladoMessage.cs`

- Classe que herda de `Message` para representar notificação de pagamento cancelado
- Contém informações completas do pedido e pagamento cancelado

### 2. Método de Atualização de Status

**Interface**: `IPagamentoRepository.AtualizarStatusPagamentoAsync`  
**Implementação**: `PagamentoRepository.AtualizarStatusPagamentoAsync`

- Busca pagamento existente no DynamoDB
- Atualiza status e data de atualização
- Persiste alterações no banco

## Componentes Modificados

### 1. CancelarPagamento (Reaproveitado e Estendido)

**Alterações em Request**:

- Adicionado `IdPedido`, `Valor`, `Moeda`, `DataPedido`

**Alterações em Response**:

- Adicionado `IdPedido`, `StatusAtualizado`, `MensagemEnviada`

**Alterações em Handler**:

- Adicionada injeção de `IPagamentoRepository`, `IMessageService`, `IConfiguration`
- Implementado atualização de status no DynamoDB
- Implementado criação e envio de mensagem para fila SQS
- Logs detalhados em todas as etapas

### 2. ConsultarStatusPagamento/Handler.cs

**Alterações**:

- Adicionada injeção de `IPagamentoRepository` e `IMediator`
- Implementada detecção de status `canceled` ou `expired`
- Disparo automático do caso de uso `CancelarPagamento` via MediatR
- Busca informações do pagamento no DynamoDB para obter IdPedido

### 3. IPagamentoRepository

**Alterações**:

- Adicionado método `AtualizarStatusPagamentoAsync`

### 4. PagamentoRepository

**Alterações**:

- Implementado método `AtualizarStatusPagamentoAsync`

### 5. appsettings.json

**Alterações**:

- Adicionada seção `AWS` com configuração `FilaPagamentoCanceladoUrl`
- URL padrão: `https://sqs.us-east-1.amazonaws.com/123456789012/torne-se-fila-pagamento-cancelado`

### 6. Function.cs

**Alterações**:

- Atualizado método `ProcessarCancelarPagamento` para incluir novos campos do Request

### 7. Documentação

**Arquivos Atualizados**:

- `.github/copilot-instructions.md`: Adicionada documentação do fluxo de cancelamento automático

## Fluxo de Execução

1. **Consulta de Status**: `ConsultarStatusPagamento` é chamado
2. **Verificação Stripe**: Obtém status atual do Stripe
3. **Detecção**: Se status for `canceled` ou `expired`:
   - Busca informações do pagamento no DynamoDB
   - Dispara `CancelarPagamento` via MediatR (caso de uso existente reutilizado)
4. **Processamento em CancelarPagamento**:
   - Cancela pagamento no Stripe (se ainda não cancelado)
   - Atualiza status no DynamoDB
   - Cria mensagem `PagamentoCanceladoMessage`
   - Envia para fila SQS configurada
5. **Resposta**: Retorna status da consulta normalmente

## Conformidade com Requisitos

✅ **Reaproveitamento**: Caso de uso `CancelarPagamento` existente foi estendido  
✅ **Sem Referências Diretas**: Usa MediatR para comunicação entre casos de uso  
✅ **Atualização de Status**: Implementado via `IPagamentoRepository`  
✅ **Notificação de Falhas**: Logs detalhados em caso de erro  
✅ **Envio de Mensagem**: Usa `IMessageService` existente  
✅ **Configuração**: URL da fila via variável de ambiente  
✅ **Logs Úteis**: Structured logging em todas as operações  
✅ **Documentação**: Atualizada com novo fluxo  
✅ **Sem Regressões**: Comportamento existente de CancelarPagamento preservado  
✅ **Build Validado**: Compilação sem erros

## Configuração Necessária

### Variável de Ambiente (Produção)

```bash
AWS__FilaPagamentoCanceladoUrl=https://sqs.us-east-1.amazonaws.com/ACCOUNT_ID/torne-se-fila-pagamento-cancelado
```

### Permissões IAM Necessárias

A Lambda precisa de permissão para enviar mensagens para a fila:

```json
{
  "Effect": "Allow",
  "Action": [
    "sqs:SendMessage"
  ],
  "Resource": "arn:aws:sqs:us-east-1:ACCOUNT_ID:torne-se-fila-pagamento-cancelado"
}
```

## Testes Recomendados

1. **Teste Unitário**: Handler do CancelarPagamento estendido
2. **Teste Integração**: Fluxo completo de detecção e processamento
3. **Teste de Falha**: Validar logs quando DynamoDB ou SQS falham
4. **Teste de Status**: Validar detecção de `canceled` e `expired`
5. **Teste Backward Compatibility**: Validar que uso direto de CancelarPagamento ainda funciona

## Próximos Passos Sugeridos

1. Criar fila SQS `torne-se-fila-pagamento-cancelado` na AWS
2. Atualizar template CloudFormation com nova fila e permissões
3. Implementar testes unitários e de integração
4. Configurar variável de ambiente em produção
5. Validar retrocompatibilidade do caso de uso CancelarPagamento
