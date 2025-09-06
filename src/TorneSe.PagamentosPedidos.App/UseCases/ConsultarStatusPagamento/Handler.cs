using MediatR;
using Microsoft.Extensions.Logging;
using TorneSe.PagamentosPedidos.App.Abstracoes.Infraestrutura;
using TorneSe.PagamentosPedidos.App.Common;
using TorneSe.PagamentosPedidos.App.UseCases.ConsultarStatusPagamento.Request;
using TorneSe.PagamentosPedidos.App.UseCases.ConsultarStatusPagamento.Response;

namespace TorneSe.PagamentosPedidos.App.UseCases.ConsultarStatusPagamento;

public sealed class Handler(ILogger<Handler> logger, IPagamentoService pagamentoService)
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
