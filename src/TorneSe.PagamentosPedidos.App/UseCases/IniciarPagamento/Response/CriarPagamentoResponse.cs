namespace TorneSe.PagamentosPedidos.App.UseCases.IniciarPagamento.Response;

public class CriarPagamentoResponse
{
    public string IdPedido { get; set; }
    public string PaymentIntentId { get; set; }
    public string Status { get; set; }
    public decimal Valor { get; set; }
    public string Moeda { get; set; }
    public DateTime DataCriacao { get; set; }
    public string UrlPagamento { get; set; }
}