using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.Extensions.Logging;
using TorneSe.PagamentosPedidos.App.Abstracoes.Infraestrutura;
using TorneSe.PagamentosPedidos.App.Infraestrutura.Models;

namespace TorneSe.PagamentosPedidos.App.Infraestrutura.Services;

public class PagamentoRepository(IAmazonDynamoDB dynamoDbClient, ILogger<PagamentoRepository> logger)
    : IPagamentoRepository
{
    private readonly IDynamoDBContext _dynamoDbContext = new DynamoDBContext(dynamoDbClient);
    private readonly ILogger<PagamentoRepository> _logger = logger;

    public async Task<bool> SalvarPagamentoAsync(PagamentoDynamoModel pagamento)
    {
        try
        {
            pagamento.DataAtualizacao = DateTime.UtcNow;
            await _dynamoDbContext.SaveAsync(pagamento);

            _logger.LogInformation("Pagamento salvo com sucesso: IdPedido={IdPedido}, PaymentIntentId={PaymentIntentId}",
                pagamento.IdPedido, pagamento.PaymentIntentId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao salvar pagamento no DynamoDB: IdPedido={IdPedido}, PaymentIntentId={PaymentIntentId}",
                pagamento.IdPedido, pagamento.PaymentIntentId);
            return false;
        }
    }

    public async Task<PagamentoDynamoModel> ObterPagamentoAsync(string idPedido, string paymentIntentId)
    {
        try
        {
            var pagamento = await _dynamoDbContext.LoadAsync<PagamentoDynamoModel>(idPedido, paymentIntentId);

            if (pagamento == null)
            {
                _logger.LogWarning("Pagamento não encontrado: IdPedido={IdPedido}, PaymentIntentId={PaymentIntentId}",
                    idPedido, paymentIntentId);
            }

            return pagamento;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar pagamento no DynamoDB: IdPedido={IdPedido}, PaymentIntentId={PaymentIntentId}",
                idPedido, paymentIntentId);
            throw;
        }
    }

    public async Task<IEnumerable<PagamentoDynamoModel>> ObterPagamentosPorPedidoAsync(string idPedido)
    {
        try
        {
            // Usando Query ao invés de Scan - mais eficiente e econômico
            var search = _dynamoDbContext.QueryAsync<PagamentoDynamoModel>(idPedido);
            var pagamentos = await search.GetRemainingAsync();

            _logger.LogInformation("Encontrados {Count} pagamentos para o pedido: {IdPedido}",
                pagamentos.Count, idPedido);

            return pagamentos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar pagamentos por pedido no DynamoDB: IdPedido={IdPedido}", idPedido);
            throw;
        }
    }
}