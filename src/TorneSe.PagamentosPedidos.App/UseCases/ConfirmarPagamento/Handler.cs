using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TorneSe.PagamentosPedidos.App.Abstracoes.Infraestrutura;
using TorneSe.PagamentosPedidos.App.Common;
using TorneSe.PagamentosPedidos.App.Domain.Messages;
using TorneSe.PagamentosPedidos.App.UseCases.ConfirmarPagamento.Request;
using TorneSe.PagamentosPedidos.App.UseCases.ConfirmarPagamento.Response;

namespace TorneSe.PagamentosPedidos.App.UseCases.ConfirmarPagamento;

/// <summary>
/// Handler para confirmar pagamento
/// </summary>
public sealed class Handler(
    ILogger<Handler> logger,
    IPagamentoRepository pagamentoRepository,
    IMessageService messageService,
    IConfiguration configuration)
    : IRequestHandler<ConfirmarPagamentoRequest, Result<ConfirmarPagamentoResponse>>
{
    public async Task<Result<ConfirmarPagamentoResponse>> Handle(ConfirmarPagamentoRequest request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Confirmando pagamento - IdPedido: {IdPedido}, PaymentIntentId: {PaymentIntentId}",
                request.IdPedido, request.PaymentIntentId);

            var dataConfirmacao = DateTime.UtcNow;
            var statusAtualizado = false;
            var mensagemEnviada = false;

            // Atualizar status do pagamento no DynamoDB para "Pago"
            statusAtualizado = await pagamentoRepository.AtualizarStatusPagamentoAsync(
                request.IdPedido,
                request.PaymentIntentId,
                "Pago");

            if (!statusAtualizado)
            {
                logger.LogError("Falha ao atualizar status do pagamento no DynamoDB - IdPedido: {IdPedido}, PaymentIntentId: {PaymentIntentId}",
                    request.IdPedido, request.PaymentIntentId);
            }

            // Obter URL da fila de pagamentos confirmados da configuração
            var filaUrl = configuration["AWS:FilaPagamentoConfirmadoUrl"];

            if (string.IsNullOrEmpty(filaUrl))
            {
                logger.LogError("URL da fila de pagamentos confirmados não configurada. Configure AWS:FilaPagamentoConfirmadoUrl");
            }
            else
            {
                // Criar mensagem de pagamento confirmado
                var mensagem = new PagamentoConfirmadoMessage
                {
                    IdPedido = request.IdPedido,
                    PaymentIntentId = request.PaymentIntentId,
                    Status = "Pago",
                    Valor = request.Valor,
                    Moeda = request.Moeda,
                    DataConfirmacao = dataConfirmacao,
                    DataPedido = request.DataPedido
                };

                // Enviar mensagem para a fila SQS
                mensagemEnviada = await messageService.SendAsync(mensagem, filaUrl);

                if (!mensagemEnviada)
                {
                    logger.LogError("Falha ao enviar mensagem de confirmação para a fila - IdPedido: {IdPedido}, PaymentIntentId: {PaymentIntentId}",
                        request.IdPedido, request.PaymentIntentId);
                }
                else
                {
                    logger.LogInformation("Mensagem de confirmação enviada com sucesso para a fila - IdPedido: {IdPedido}",
                        request.IdPedido);
                }
            }

            var response = new ConfirmarPagamentoResponse
            {
                IdPedido = request.IdPedido,
                PaymentIntentId = request.PaymentIntentId,
                Confirmado = statusAtualizado,
                DataConfirmacao = dataConfirmacao,
                StatusAtualizado = statusAtualizado,
                MensagemEnviada = mensagemEnviada
            };

            logger.LogInformation("Confirmação processada - IdPedido: {IdPedido}, StatusAtualizado: {StatusAtualizado}, MensagemEnviada: {MensagemEnviada}",
                request.IdPedido, statusAtualizado, mensagemEnviada);

            return Result<ConfirmarPagamentoResponse>.Success(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao confirmar pagamento - IdPedido: {IdPedido}, PaymentIntentId: {PaymentIntentId}",
                request.IdPedido, request.PaymentIntentId);
            return Result<ConfirmarPagamentoResponse>.Error($"Erro interno: {ex.Message}");
        }
    }
}
