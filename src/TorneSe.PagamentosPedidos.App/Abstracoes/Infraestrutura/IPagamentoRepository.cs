using TorneSe.PagamentosPedidos.App.Infraestrutura.Models;

namespace TorneSe.PagamentosPedidos.App.Abstracoes.Infraestrutura;

public interface IPagamentoRepository
{
    Task<bool> SalvarPagamentoAsync(PagamentoDynamoModel pagamento);
    Task<PagamentoDynamoModel> ObterPagamentoAsync(string idPedido, string paymentIntentId);
    Task<IEnumerable<PagamentoDynamoModel>> ObterPagamentosPorPedidoAsync(string idPedido);
}