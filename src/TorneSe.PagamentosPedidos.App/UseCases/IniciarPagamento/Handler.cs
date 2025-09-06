using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using TorneSe.PagamentosPedidos.App.Abstracoes.Infraestrutura;
using TorneSe.PagamentosPedidos.App.Common;
using TorneSe.PagamentosPedidos.App.UseCases.IniciarPagamento.Request;
using TorneSe.PagamentosPedidos.App.UseCases.IniciarPagamento.Response;

namespace TorneSe.PagamentosPedidos.App.UseCases.IniciarPagamento;

public sealed class Handler(
    ILogger<Handler> logger, 
    IDbService dbService,
    IPagamentoService pagamentoService,
    IMapper mapper)
    : IRequestHandler<CriarPagamentoRequest, Result<CriarPagamentoResponse>>
{
    public async Task<Result<CriarPagamentoResponse>> Handle(CriarPagamentoRequest request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Processando pagamento - IdPedido: {IdPedido}, DataPedido: {DataPedido}", 
                request.IdPedido, request.DataPedido.ToString("yyyy-MM-dd"));

            // Buscar dados do pedido no DynamoDB usando as chaves primárias
            var dataPedidoFormatada = request.DataPedido.ToString("yyyy-MM-dd");
            var pedidoDynamo = await dbService.ObterPedidoAsync(dataPedidoFormatada, request.IdPedido);
            
            if (pedidoDynamo == null)
            {
                logger.LogError("Pedido não encontrado no DynamoDB: DataPedido={DataPedido}, IdPedido={IdPedido}", 
                    dataPedidoFormatada, request.IdPedido);
                return Result<CriarPagamentoResponse>.Error($"Pedido não encontrado: {request.IdPedido}");
            }

            // Mapear dados do pedido para DTO de pagamento usando AutoMapper
            var pagamentoDto = mapper.Map<CriarPagamentoDto>(pedidoDynamo);

            // Criar pagamento no Stripe
            var resultadoPagamento = await pagamentoService.CriarPagamentoAsync(pagamentoDto);
            
            if (!resultadoPagamento.IsSuccess)
            {
                logger.LogError("Falha ao criar pagamento no Stripe: {Erro}", resultadoPagamento.Message);
                return Result<CriarPagamentoResponse>.Error(resultadoPagamento.Message);
            }

            var paymentIntentId = resultadoPagamento.Data;

            // Obter status do pagamento
            var resultadoStatus = await pagamentoService.ObterStatusPagamentoAsync(paymentIntentId);
            
            if (!resultadoStatus.IsSuccess)
            {
                logger.LogError("Falha ao obter status do pagamento: {Erro}", resultadoStatus.Message);
                return Result<CriarPagamentoResponse>.Error(resultadoStatus.Message);
            }

            var statusPagamento = resultadoStatus.Data;

            // Criar resposta
            var response = new CriarPagamentoResponse
            {
                IdPedido = request.IdPedido,
                PaymentIntentId = paymentIntentId,
                Status = statusPagamento.Status,
                Valor = statusPagamento.Valor,
                Moeda = statusPagamento.Moeda,
                DataCriacao = statusPagamento.DataCriacao,
                UrlPagamento = $"https://checkout.stripe.com/pay/{paymentIntentId}" // URL de exemplo
            };

            return Result<CriarPagamentoResponse>.Success(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao processar pagamento para o pedido: {IdPedido}", request.IdPedido);
            return Result<CriarPagamentoResponse>.Error($"Erro interno: {ex.Message}");
        }
    }
}