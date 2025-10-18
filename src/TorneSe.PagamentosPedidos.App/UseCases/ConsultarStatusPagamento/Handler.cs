using MediatR;
using Microsoft.Extensions.Logging;
using TorneSe.PagamentosPedidos.App.Abstracoes.Infraestrutura;
using TorneSe.PagamentosPedidos.App.Common;
using TorneSe.PagamentosPedidos.App.UseCases.ConsultarStatusPagamento.Request;
using TorneSe.PagamentosPedidos.App.UseCases.ConsultarStatusPagamento.Response;
using TorneSe.PagamentosPedidos.App.UseCases.CancelarPagamento.Request;
using TorneSe.PagamentosPedidos.App.UseCases.ConfirmarPagamento.Request;

namespace TorneSe.PagamentosPedidos.App.UseCases.ConsultarStatusPagamento;

public sealed class Handler(
    ILogger<Handler> logger,
    IPagamentoService pagamentoService,
    IPagamentoRepository pagamentoRepository,
    IMediator mediator)
    : IRequestHandler<ConsultarStatusPagamentoRequest, Result<ConsultarStatusPagamentoResponse>>
{
    public async Task<Result<ConsultarStatusPagamentoResponse>> Handle(ConsultarStatusPagamentoRequest request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Consultando status - PaymentIntentId: {PaymentIntentId}", request.PaymentIntentId);

            var resultadoStatus = await pagamentoService.ObterStatusPagamentoAsync(request.PaymentIntentId);

            if (!resultadoStatus.IsSuccess)
            {
                logger.LogError("Falha ao obter status do pagamento: {Erro}", resultadoStatus.Message);
                return Result<ConsultarStatusPagamentoResponse>.Error(resultadoStatus.Message);
            }

            var statusPagamento = resultadoStatus.Data;

            // Verificar se o pagamento foi cancelado ou expirado
            if (statusPagamento.Status == "canceled" || statusPagamento.Status == "expired")
            {
                logger.LogInformation("Pagamento cancelado/expirado detectado - PaymentIntentId: {PaymentIntentId}, Status: {Status}",
                    request.PaymentIntentId, statusPagamento.Status);

                // Buscar informações do pagamento no DynamoDB para obter IdPedido
                var pagamentos = await pagamentoRepository.ObterPagamentosPorPedidoAsync(request.PaymentIntentId);
                var pagamento = pagamentos.FirstOrDefault(p => p.PaymentIntentId == request.PaymentIntentId);

                if (pagamento != null)
                {
                    // Disparar caso de uso CancelarPagamento via MediatR
                    var cancelarPagamentoRequest = new CancelarPagamentoRequest
                    {
                        IdPedido = pagamento.IdPedido,
                        PaymentIntentId = request.PaymentIntentId,
                        MotivoCancelamento = statusPagamento.Status == "expired"
                            ? "Pagamento expirado"
                            : "Pagamento cancelado",
                        Valor = statusPagamento.Valor,
                        Moeda = statusPagamento.Moeda,
                        DataPedido = pagamento.DataCriacao
                    };

                    var resultadoCancelamento = await mediator.Send(cancelarPagamentoRequest, cancellationToken);

                    if (!resultadoCancelamento.IsSuccess)
                    {
                        logger.LogWarning("Falha ao processar cancelamento do pagamento - PaymentIntentId: {PaymentIntentId}, Erro: {Erro}",
                            request.PaymentIntentId, resultadoCancelamento.Message);
                    }
                }
                else
                {
                    logger.LogWarning("Pagamento cancelado/expirado não encontrado no DynamoDB - PaymentIntentId: {PaymentIntentId}",
                        request.PaymentIntentId);
                }
            }

            // Verificar se o pagamento foi confirmado/sucedido
            if (statusPagamento.Status == "succeeded")
            {
                logger.LogInformation("Pagamento confirmado detectado - PaymentIntentId: {PaymentIntentId}, Status: {Status}",
                    request.PaymentIntentId, statusPagamento.Status);

                // Buscar informações do pagamento no DynamoDB para obter IdPedido
                var pagamentos = await pagamentoRepository.ObterPagamentosPorPedidoAsync(request.PaymentIntentId);
                var pagamento = pagamentos.FirstOrDefault(p => p.PaymentIntentId == request.PaymentIntentId);

                if (pagamento != null)
                {
                    // Disparar caso de uso ConfirmarPagamento via MediatR
                    var confirmarPagamentoRequest = new ConfirmarPagamentoRequest
                    {
                        IdPedido = pagamento.IdPedido,
                        PaymentIntentId = request.PaymentIntentId,
                        Valor = statusPagamento.Valor,
                        Moeda = statusPagamento.Moeda,
                        DataPedido = pagamento.DataCriacao
                    };

                    var resultadoConfirmacao = await mediator.Send(confirmarPagamentoRequest, cancellationToken);

                    if (!resultadoConfirmacao.IsSuccess)
                    {
                        logger.LogWarning("Falha ao processar confirmação do pagamento - PaymentIntentId: {PaymentIntentId}, Erro: {Erro}",
                            request.PaymentIntentId, resultadoConfirmacao.Message);
                    }
                }
                else
                {
                    logger.LogWarning("Pagamento confirmado não encontrado no DynamoDB - PaymentIntentId: {PaymentIntentId}",
                        request.PaymentIntentId);
                }
            }

            var response = new ConsultarStatusPagamentoResponse
            {
                PaymentIntentId = statusPagamento.PaymentIntentId,
                Status = statusPagamento.Status,
                Valor = statusPagamento.Valor,
                Moeda = statusPagamento.Moeda,
                DataCriacao = statusPagamento.DataCriacao,
                DataConfirmacao = statusPagamento.DataConfirmacao,
                IdPedido = statusPagamento.PaymentIntentId, // Pode ser extraído dos metadados se necessário
                NomeCliente = string.Empty // Pode ser extraído dos metadados se necessário
            };

            return Result<ConsultarStatusPagamentoResponse>.Success(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao consultar status do pagamento: {PaymentIntentId}", request.PaymentIntentId);
            return Result<ConsultarStatusPagamentoResponse>.Error($"Erro interno: {ex.Message}");
        }
    }
}
