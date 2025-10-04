using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Logging;
using TorneSe.PagamentosPedidos.App.Abstracoes.Infraestrutura;
using TorneSe.PagamentosPedidos.App.Infraestrutura.Models;

namespace TorneSe.PagamentosPedidos.App.Infraestrutura.Services;

public class PedidoRepository(IAmazonDynamoDB dynamoDbClient, ILogger<PedidoRepository> logger)
    : IPedidoRepository
{
    private readonly IDynamoDBContext _dynamoDbContext = new DynamoDBContext(dynamoDbClient);
    private readonly ILogger<PedidoRepository> _logger = logger;

    public async Task<bool> SalvarPedidoAsync(PedidoDynamoModel pedido)
    {
        try
        {
            await _dynamoDbContext.SaveAsync(pedido);

            _logger.LogInformation("Pedido salvo com sucesso: DataPedido={DataPedido}, Id={Id}",
                pedido.DataPedido, pedido.Id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao salvar pedido no DynamoDB: DataPedido={DataPedido}, Id={Id}",
                pedido.DataPedido, pedido.Id);
            return false;
        }
    }

    public async Task<PedidoDynamoModel> ObterPedidoAsync(string dataPedido, string idPedido)
    {
        try
        {
            // Usando LoadAsync com as chaves primárias - operação eficiente
            var pedido = await _dynamoDbContext.LoadAsync<PedidoDynamoModel>(dataPedido, idPedido);

            if (pedido == null)
            {
                _logger.LogWarning("Pedido não encontrado: DataPedido={DataPedido}, IdPedido={IdPedido}",
                    dataPedido, idPedido);
            }

            return pedido;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar pedido no DynamoDB: DataPedido={DataPedido}, IdPedido={IdPedido}",
                dataPedido, idPedido);
            throw;
        }
    }

    public async Task<IEnumerable<PedidoDynamoModel>> ObterPedidosPorDataAsync(string dataPedido)
    {
        try
        {
            // Usando Query ao invés de Scan - mais eficiente e econômico
            var search = _dynamoDbContext.QueryAsync<PedidoDynamoModel>(dataPedido);
            var pedidos = await search.GetRemainingAsync();

            _logger.LogInformation("Encontrados {Count} pedidos para a data: {DataPedido}",
                pedidos.Count, dataPedido);

            return pedidos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar pedidos por data no DynamoDB: DataPedido={DataPedido}", dataPedido);
            throw;
        }
    }
}