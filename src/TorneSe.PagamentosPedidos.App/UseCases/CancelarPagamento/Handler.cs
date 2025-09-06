using MediatR;
using Microsoft.Extensions.Logging;
using TorneSe.PagamentosPedidos.App.Abstracoes.Infraestrutura;
using TorneSe.PagamentosPedidos.App.Common;
using TorneSe.PagamentosPedidos.App.UseCases.CancelarPagamento.Request;
using TorneSe.PagamentosPedidos.App.UseCases.CancelarPagamento.Response;

namespace TorneSe.PagamentosPedidos.App.UseCases.CancelarPagamento;

public sealed class Handler(ILogger<Handler> logger, IPagamentoService pagamentoService)
    : IRequestHandler<CancelarPagamentoRequest, Result<CancelarPagamentoResponse>>
{
    public async Task<Result<CancelarPagamentoResponse>> Handle(CancelarPagamentoRequest request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Cancelando pagamento - PaymentIntentId: {PaymentIntentId}, Motivo: {Motivo}", 
                request.PaymentIntentId, request.MotivoCancelamento);

            var resultadoCancelamento = await pagamentoService.CancelarPagamentoAsync(request.PaymentIntentId);
            
            if (!resultadoCancelamento.IsSuccess)
            {
                logger.LogError("Falha ao cancelar pagamento: {Erro}", resultadoCancelamento.Message);
                return Result<CancelarPagamentoResponse>.Error(resultadoCancelamento.Message);
            }

            var response = new CancelarPagamentoResponse
            {
                PaymentIntentId = request.PaymentIntentId,
                Cancelado = resultadoCancelamento.Data,
                DataCancelamento = DateTime.UtcNow,
                MotivoCancelamento = request.MotivoCancelamento
            };

            return Result<CancelarPagamentoResponse>.Success(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao cancelar pagamento: {PaymentIntentId}", request.PaymentIntentId);
            return Result<CancelarPagamentoResponse>.Error($"Erro interno: {ex.Message}");
        }
    }
}
