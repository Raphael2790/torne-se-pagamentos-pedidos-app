# Feature: Implementar Cancelamento de Pagamento através de consulta

## Objetivo

Ao realizar uma consulta executando o caso de uso "ConsultarStatusPagamento" caso o status do pagamento tenha o status de pagamento "cancelado" de acordo com o payload recebido da Stripe deve mudar o status do pagamento para cancelado e enviar a confirmação para a fila de pagamento cancelado

### Contexto

- Ao realizar uma consulta de status pagamento e o estado do pagamento esteja como cancelado (ou de acordo com a condição que indica pagamento cancelado ou expirado pela Stripe)
- Deve ser criado um caso de uso separado para manipular a mudança de estados do pagamento para cancelado ou expirado
- Deve ser alterado o status do pagamento no DynamoDb para cancelado ou expirado
- Deve ser enviado o pagamento cancelado para uma fila AWS SQS para notificar os interessados no recebimento do pagamento
  
---

### Passos

1. **Organizar Estrutura da Aplicação**
   - Criar estrutura de pastas para novo caso de uso de "CancelarPagamento", seguinte a estrutura dos demais.
   - Criar classes de Request, Response e Handler necessários.
   - Não devem ser adicionadas referencias diretas entre casos de uso, deve ser utilizado o MediatR
   - Criar um serviço para encapsular a lógica de cancelamento de pagamento.

2. **Alterar o status do pagamento no caso de uso CancelarPagamento**
   - Alterar o status do pagamento na tabela do DynamoDb "Pagamentos" para "Cancelado".
   - Caso falhe a alteração deverá haver uma notificação de algum forma que ocorreu um erro e efetuado logs.
   - Utilizar o serviço existente IPagamentoRepository, caso não exista a implementação necessária criar

3. **Enviar mensagem de cancelamento de pagamento no caso de uso CancelarPagamento**
   - Publicar mensagem no AWS SQS na fila "torne-se-fila-pagamento-cancelado"
   - A mensagem deve ter informações do pedido e o status do pagamento, modelar uma classe mensagem no dominio da aplicação
   - A url da fila deve ser recuperada das variáveis de ambiente da aplicação
   - Utilizar o serviços existente de comunicação com AWS através da interface IMessageService
   - Caso seja necessário recuperar informações do pedido utilize o IPedidoRepository
  
---

### Critérios de Aceite

- A implementação deve seguir as convenções e práticas definidas no repositório
- Deve ser criado testes unitários e de integração cobrindo os novos casos de uso
- Deve ser criado logs úteis para rastreamento e diagnóstico
- Deve ser utilizado o MediatR para comunicação entre casos de uso
- A implementação deve ser documentada adequadamente, incluindo comentários no código e atualizações na documentação do projeto, se necessário
- Deve ser considerado o impacto em outros casos de uso e garantir que não haja regressões
  
---

### Restrições

- Seguir apenas as convenções, arquitetura e práticas de estrutura previamente definidas.
- Não alterar o comportamento de casos de uso existentes
- Não adicionar dependências desnecessárias ou complexidade ao projeto
- Garantir que a solução seja escalável e mantenível
- Evitar duplicação de código, reutilizando componentes existentes sempre que possível
- Garantir que a solução seja segura, evitando vulnerabilidades comuns
