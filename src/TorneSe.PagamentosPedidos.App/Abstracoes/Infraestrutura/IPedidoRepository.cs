using TorneSe.PagamentosPedidos.App.Infraestrutura.Models;

namespace TorneSe.PagamentosPedidos.App.Abstracoes.Infraestrutura;

public interface IPedidoRepository
{
    Task<bool> SalvarPedidoAsync(PedidoDynamoModel pedido);
    Task<PedidoDynamoModel> ObterPedidoAsync(string dataPedido, string idPedido);
    Task<IEnumerable<PedidoDynamoModel>> ObterPedidosPorDataAsync(string dataPedido);
}