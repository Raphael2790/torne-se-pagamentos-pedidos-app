using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
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

    public async Task<PedidoDynamoModel> ObterPedidoAsync(string idPedido)
    {
        try
        {
            _logger.LogInformation("Consultando pedido no DynamoDB: {IdPedido}", idPedido);

            // Buscar o pedido no DynamoDB usando Scan (não ideal para produção, mas funcional)
            // Em produção, considere usar um GSI ou reorganizar a estrutura da tabela
            var scanConditions = new List<ScanCondition>
            {
                new ScanCondition("Id", ScanOperator.Equal, idPedido)
            };

            var pedidos = await _dynamoDbContext.ScanAsync<PedidoDynamoModel>(scanConditions).GetRemainingAsync();

            var pedido = pedidos.FirstOrDefault();

            if (pedido == null)
            {
                _logger.LogWarning("Pedido não encontrado: {IdPedido}", idPedido);
                return null;
            }

            _logger.LogInformation("Pedido encontrado: {IdPedido}, Status: {Status}", idPedido, pedido.Status);

            return pedido;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar pedido no DynamoDB: {IdPedido}", idPedido);
            throw;
        }
    }
}