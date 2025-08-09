namespace TorneSe.PagamentosPedidos.App.UseCases.IniciarPagamento.Request;

public class CriarPagamentoRequest
{
    public string IdPedido { get; set; }
    public string Status { get; set; }
    public DateTime DataPedido { get; set; }
    public DateTime DataHoraEvento { get; set; }
}