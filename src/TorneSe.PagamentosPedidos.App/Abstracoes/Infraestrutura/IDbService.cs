using TorneSe.PagamentosPedidos.App.Infraestrutura.Models;

namespace TorneSe.PagamentosPedidos.App.Abstracoes.Infraestrutura;

public interface IDbService
{
    Task<bool> SaveAsync<T>(T entity);
    Task<PedidoDynamoModel> ObterPedidoAsync(string idPedido);
}