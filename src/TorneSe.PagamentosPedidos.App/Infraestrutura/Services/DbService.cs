using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Logging;
using TorneSe.PagamentosPedidos.App.Abstracoes.Infraestrutura;
using TorneSe.PagamentosPedidos.App.Infraestrutura.Models;

namespace TorneSe.PagamentosPedidos.App.Infraestrutura.Services;

public class DbService : IDbService
{
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly IDynamoDBContext _dynamoDbContext;
    private readonly ILogger<DbService> _logger;

    public DbService(IAmazonDynamoDB dynamoDbClient, ILogger<DbService> logger)
    {
        _dynamoDbClient = dynamoDbClient;
        _dynamoDbContext = new DynamoDBContext(_dynamoDbClient);
        _logger = logger;
    }

    public async Task<bool> SaveAsync<T>(T entity)
    {
        try
        {
            await _dynamoDbContext.SaveAsync(entity);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao salvar entidade no DynamoDB");
            return false;
        }
    }

    public async Task<PedidoDynamoModel> ObterPedidoAsync(string dataPedido, string idPedido)
    {
        try
        {
            // Buscar o pedido no DynamoDB usando Query com as chaves primárias
            var pedido = await _dynamoDbContext.LoadAsync<PedidoDynamoModel>(dataPedido, idPedido);

            if (pedido is null)
            {
                _logger.LogWarning("Pedido não encontrado: DataPedido={DataPedido}, IdPedido={IdPedido}", dataPedido, idPedido);
                return null;
            }

            return pedido;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar pedido no DynamoDB: DataPedido={DataPedido}, IdPedido={IdPedido}", dataPedido, idPedido);
            throw;
        }
    }
}