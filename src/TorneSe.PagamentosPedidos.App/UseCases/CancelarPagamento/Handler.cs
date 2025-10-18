using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TorneSe.PagamentosPedidos.App.Abstracoes.Infraestrutura;
using TorneSe.PagamentosPedidos.App.Common;
using TorneSe.PagamentosPedidos.App.Domain.Messages;
using TorneSe.PagamentosPedidos.App.UseCases.CancelarPagamento.Request;
using TorneSe.PagamentosPedidos.App.UseCases.CancelarPagamento.Response;

namespace TorneSe.PagamentosPedidos.App.UseCases.CancelarPagamento;

public sealed class Handler(
    ILogger<Handler> logger,
    IPagamentoService pagamentoService,
    IPagamentoRepository pagamentoRepository,
    IMessageService messageService,
    IConfiguration configuration)
    : IRequestHandler<CancelarPagamentoRequest, Result<CancelarPagamentoResponse>>
{
    public async Task<Result<CancelarPagamentoResponse>> Handle(CancelarPagamentoRequest request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Cancelando pagamento - IdPedido: {IdPedido}, PaymentIntentId: {PaymentIntentId}, Motivo: {Motivo}",
                request.IdPedido, request.PaymentIntentId, request.MotivoCancelamento);

            var dataCancelamento = DateTime.UtcNow;
            var statusAtualizado = false;
            var mensagemEnviada = false;

            // Cancelar pagamento no Stripe (se ainda não estiver cancelado)
            var resultadoCancelamento = await pagamentoService.CancelarPagamentoAsync(request.PaymentIntentId);

            if (!resultadoCancelamento.IsSuccess)
            {
                logger.LogWarning("Falha ao cancelar pagamento no Stripe (pode já estar cancelado): {Erro}", resultadoCancelamento.Message);
            }

            // Atualizar status do pagamento no DynamoDB
            statusAtualizado = await pagamentoRepository.AtualizarStatusPagamentoAsync(
                request.IdPedido,
                request.PaymentIntentId,
                "Cancelado");

            if (!statusAtualizado)
            {
                logger.LogError("Falha ao atualizar status do pagamento no DynamoDB - IdPedido: {IdPedido}, PaymentIntentId: {PaymentIntentId}",
                    request.IdPedido, request.PaymentIntentId);
            }

            // Obter URL da fila de pagamentos cancelados da configuração
            var filaUrl = configuration["AWS:FilaPagamentoCanceladoUrl"];

            if (string.IsNullOrEmpty(filaUrl))
            {
                logger.LogError("URL da fila de pagamentos cancelados não configurada. Configure AWS:FilaPagamentoCanceladoUrl");
            }
            else
            {
                // Criar mensagem de pagamento cancelado
                var mensagem = new PagamentoCanceladoMessage
                {
                    IdPedido = request.IdPedido,
                    PaymentIntentId = request.PaymentIntentId,
                    Status = "Cancelado",
                    Valor = request.Valor,
                    Moeda = request.Moeda,
                    DataCancelamento = dataCancelamento,
                    MotivoCancelamento = request.MotivoCancelamento ?? "Cancelamento solicitado",
                    DataPedido = request.DataPedido
                };

                // Enviar mensagem para a fila SQS
                mensagemEnviada = await messageService.SendAsync(mensagem, filaUrl);

                if (!mensagemEnviada)
                {
                    logger.LogError("Falha ao enviar mensagem de cancelamento para a fila - IdPedido: {IdPedido}, PaymentIntentId: {PaymentIntentId}",
                        request.IdPedido, request.PaymentIntentId);
                }
                else
                {
                    logger.LogInformation("Mensagem de cancelamento enviada com sucesso para a fila - IdPedido: {IdPedido}",
                        request.IdPedido);
                }
            }

            var response = new CancelarPagamentoResponse
            {
                IdPedido = request.IdPedido,
                PaymentIntentId = request.PaymentIntentId,
                Cancelado = resultadoCancelamento.IsSuccess || statusAtualizado,
                DataCancelamento = dataCancelamento,
                MotivoCancelamento = request.MotivoCancelamento,
                StatusAtualizado = statusAtualizado,
                MensagemEnviada = mensagemEnviada
            };

            logger.LogInformation("Cancelamento processado - IdPedido: {IdPedido}, StatusAtualizado: {StatusAtualizado}, MensagemEnviada: {MensagemEnviada}",
                request.IdPedido, statusAtualizado, mensagemEnviada);

            return Result<CancelarPagamentoResponse>.Success(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao cancelar pagamento: {PaymentIntentId}", request.PaymentIntentId);
            return Result<CancelarPagamentoResponse>.Error($"Erro interno: {ex.Message}");
        }
    }
}
